// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Steeltoe.Management.Endpoint.ContentNegotiation;
using Steeltoe.Management.Endpoint.Middleware;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steeltoe.Management.Endpoint.Refresh;

public class RefreshEndpointMiddleware : EndpointMiddleware<IList<string>>
{
    private readonly RequestDelegate _next;

    public RefreshEndpointMiddleware(RequestDelegate next, RefreshEndpoint endpoint, IManagementOptions managementOptions, ILogger<RefreshEndpointMiddleware> logger = null)
        : base(endpoint, managementOptions, logger: logger)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        if (endpoint.ShouldInvoke(managementOptions, logger))
        {
            return HandleRefreshRequestAsync(context);
        }

        return Task.CompletedTask;
    }

    protected internal Task HandleRefreshRequestAsync(HttpContext context)
    {
        var serialInfo = HandleRequest();
        logger?.LogDebug("Returning: {0}", serialInfo);

        context.HandleContentNegotiation(logger);
        return context.Response.WriteAsync(serialInfo);
    }
}
