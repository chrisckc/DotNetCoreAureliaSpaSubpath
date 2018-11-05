import {config} from './build';
import configureEnvironment from './environment';
import * as webpack from 'webpack';
import * as Server from 'webpack-dev-server';
import * as project from '../aurelia.json';
import {CLIOptions, reportWebpackReadiness} from 'aurelia-cli';
import * as gulp from 'gulp';

function runWebpack(done) {
  // https://webpack.github.io/docs/webpack-dev-server.html
  let opts = {
    host: 'localhost',
    publicPath: config.output.publicPath,
    filename: config.output.filename,
    hot: project.platform.hmr || CLIOptions.hasFlag('hmr'),
    port: project.platform.port,
    //contentBase: config.output.path,
    // prevent the dev server from serving files in the 'dist' dir, may cause issues if files left over from production builds etc.
    contentBase: false,
    historyApiFallback: true,
    open: project.platform.open,
    stats: {
      colors: require('supports-color')
    },
    https: config.devServer.https
  } as any;

  if (project.platform.hmr || CLIOptions.hasFlag('hmr')) {
    config.plugins.push(new webpack.HotModuleReplacementPlugin());
    // Make HMR show correct file names in console on update.
    config.plugins.push(new webpack.NamedModulesPlugin()),
    //config.entry.app.unshift(`webpack-dev-server/client?http://${opts.host}:${opts.port}/`, 'webpack/hot/dev-server');
    // fixes issue with HMR ref: https://github.com/aspnet/JavaScriptServices/issues/1743
    // until https://github.com/webpack/webpack-dev-server/pull/1553 is released, the server end can only listen at /sockjs-node
    config.entry.app.unshift(`webpack-dev-server/client?/`, 'webpack/hot/dev-server');
  } else {
    // removed "<script src="/webpack-dev-server.js"></script>" from index.ejs and added this instead
    config.entry.app.unshift(`webpack-dev-server/client?/`);
  }

  const compiler = webpack(config);
  let server = new Server(compiler, opts);

  server.listen(opts.port, opts.host, function(err) {
    if (err) throw err;

    reportWebpackReadiness(opts);
    done();
  });
}

const run = gulp.series(
  configureEnvironment,
  runWebpack
);

export { run as default };
