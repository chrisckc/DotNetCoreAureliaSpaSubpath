// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Http;

namespace dotnetcore21_spa.Middleware
{
    /// <summary>
    /// Represents a middleware that maps a request path to a sub-request pipeline.
    /// </summary>
    public class MapPathMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MapPathOptions _options;

        /// <summary>
        /// Creates a new instance of <see cref="MapPathMiddleware"/>.
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options">The middleware options.</param>
        public MapPathMiddleware(RequestDelegate next, MapPathOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _options = options;
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A task that represents the execution of this middleware.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            PathString matchedPath;
            PathString remainingPath;
            if (context.Request.Path.StartsWithSegments(_options.PathMatch, out matchedPath, out remainingPath))
            {
                var path = context.Request.Path;
                var pathBase = context.Request.PathBase;
                if (_options.RemoveMatchedPathSegment) {
                    // Update the path
                    context.Request.PathBase = pathBase.Add(matchedPath);
                    context.Request.Path = remainingPath;
                }

                try
                {
                    await _options.Branch(context);
                }
                finally
                {
                    context.Request.PathBase = pathBase;
                    context.Request.Path = path;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }

    //
    // Summary:
    //     Options for the Microsoft.AspNetCore.Builder.Extensions.MapMiddleware.
    /// <summary>
    /// Options for the <see cref="MapPathMiddleware"/>.
    /// </summary>
    public class MapPathOptions
    {
        /// <summary>
        /// The path to match.
        /// </summary>
        public PathString PathMatch { get; set; }

        /// <summary>
        /// Whether to remove the matched path segment
        /// </summary>
        public bool RemoveMatchedPathSegment  { get; set; }

        /// <summary>
        /// The branch taken for a positive match.
        /// </summary>
        public RequestDelegate Branch { get; set; }
    }
}