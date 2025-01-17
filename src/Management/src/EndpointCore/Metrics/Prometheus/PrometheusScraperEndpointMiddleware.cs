// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Steeltoe.Management.Endpoint.Middleware;
using System.Net;
using System.Threading.Tasks;

namespace Steeltoe.Management.Endpoint.Metrics;

public class PrometheusScraperEndpointMiddleware : EndpointMiddleware<string>
{
    private readonly RequestDelegate _next;

    public PrometheusScraperEndpointMiddleware(RequestDelegate next, PrometheusScraperEndpoint endpoint, IManagementOptions managementOptions, ILogger<PrometheusScraperEndpointMiddleware> logger = null)
        : base(endpoint, managementOptions, logger)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        if (endpoint.ShouldInvoke(managementOptions, logger))
        {
            return HandleMetricsRequestAsync(context);
        }

        return Task.CompletedTask;
    }

    public override string HandleRequest()
    {
        var result = endpoint.Invoke();
        return result;
    }

    protected internal Task HandleMetricsRequestAsync(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;

        logger?.LogDebug("Incoming path: {0}", request.Path.Value);

        // GET /metrics/{metricName}?tag=key:value&tag=key:value
        var serialInfo = HandleRequest();

        if (serialInfo == null)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            return Task.CompletedTask;
        }

        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "text/plain; version=0.0.4;";
        return context.Response.WriteAsync(serialInfo);
    }
}
