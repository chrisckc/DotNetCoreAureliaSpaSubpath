using dotnetcore21_spa.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

namespace dotnetcore21_spa
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            // Debugging info to help with running in Docker
            string defaultCertPath = configuration.GetSection("Kestrel:Certificates:Default:Path").Value;
            logger.LogInformation($"Kestrel Default cert path: {defaultCertPath}");
            if (!string.IsNullOrEmpty(defaultCertPath)) {
                if (File.Exists(defaultCertPath)) {
                    logger.LogInformation("Default Cert file exists");
                } else {
                    logger.LogInformation("Default Cert file does NOT exist!"); 
                } 
            }  
            logger.LogInformation($"Kestrel Default cert pass: {configuration.GetSection("Kestrel:Certificates:Default:Password").Value}");
            
            string devCertPath = configuration.GetSection("Kestrel:Certificates:Development:Path").Value;
            logger.LogInformation($"Kestrel Development cert path: {devCertPath}");
            if (!string.IsNullOrEmpty(devCertPath)) {
                if (File.Exists(devCertPath)) {
                    logger.LogInformation("Development Cert file exists");
                } else {
                    logger.LogInformation("Development Cert file does NOT exist!"); 
                }   
            }
            logger.LogInformation($"Kestrel Development cert pass: {configuration.GetSection("Kestrel:Certificates:Development:Password").Value}");

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            // Captures webpack-dev-server related requests such as
            // websocket requests (/sockjs-node/) requests to the webpack dev server
            // These requests are made outside of '/app' sub-path so capture them here
            // These websocket requests cant be changed to use a sub-path until this has been released
            // https://github.com/webpack/webpack-dev-server/pull/1553
            // however to use that fix with Angular would require ejecting from the CLI
            app.MapWhen(context => WebPackDevServerMatcher(context), webpackDevServer => {
                webpackDevServer.UseSpa(spa => {
                    spa.UseProxyToSpaDevelopmentServer(baseUri: "http://localhost:8080");
                });
            });


            // Serve the Angular app from a sub-path '/app' off the root
            // Map the path segment '/app' to the Spa middleware
            // Custom middleware MapPath has additional option 'removeMatchedPathSegment'
            // In development we need to keep the matched path segment
            // as the request is proxied to the Angular dev server which is set to serve files from /app/
            // In production we remove the matched segment as the files exist in the root of the 'ClientApp/dist' dir.
            app.MapPath("/app", !env.IsDevelopment(), frontendApp => {
                if (!env.IsDevelopment()) {
                    // In Production env, ClientApp is served using minified and bundled code from 'ClientApp/dist'
                    frontendApp.UseSpaStaticFiles();
                }
                frontendApp.UseSpa(spa => {
                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment()) {
                        // Aurelia Webpack Dev Server runs on port 8080
                        spa.UseProxyToSpaDevelopmentServer(baseUri: "http://localhost:8080");
                    }
                });
            });
        }

        // Captures the websocket requests generated when using webpack dev server in the following ways:
        // via: https://localhost:5001/app/ (inline mode)
        // via: https://localhost:5001/webpack-dev-server/app/  (iframe mode)
        // captures requests like these:
        // https://localhost:5001/webpack-dev-server/app/
        // https://localhost:5001/__webpack_dev_server__/live.bundle.js
        // wss://localhost:5001/sockjs-node/978/qhjp11ck/websocket
        private bool WebPackDevServerMatcher(HttpContext context) {
            return context.Request.Path.StartsWithSegments("/webpack-dev-server") ||
                context.Request.Path.StartsWithSegments("/__webpack_dev_server__") ||
                context.Request.Path.StartsWithSegments("/sockjs-node");
        }
    }
}
