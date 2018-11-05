// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Builder;
using dotnetcore21_spa.Middleware;

namespace dotnetcore21_spa.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="MapPathMiddleware"/>.
    /// </summary>
    public static class MapPathExtensions
    {
        /// <summary>
        /// Branches the request pipeline based on matches of the given request path. If the request path starts with
        /// the given path, the branch is executed.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="pathMatch">The request path to match.</param>
        /// <param name="removeMatchedPathSegment">Whether to remove the matched path segment.</param>
        /// <param name="configuration">The branch to take for positive path matches.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IApplicationBuilder MapPath(this IApplicationBuilder app, PathString pathMatch, bool removeMatchedPathSegment, Action<IApplicationBuilder> configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (pathMatch.HasValue && pathMatch.Value.EndsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("The path must not end with a '/'", nameof(pathMatch));
            }

            // create branch
            var branchBuilder = app.New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();

            var options = new MapPathOptions
            {
                Branch = branch,
                RemoveMatchedPathSegment = removeMatchedPathSegment,
                PathMatch = pathMatch,
            };
            return app.Use(next => new MapPathMiddleware(next, options).Invoke);
        }
    }
}