// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Steeltoe.Management.Endpoint.ContentNegotiation;
using Steeltoe.Management.Endpoint.Info;
using Steeltoe.Management.Endpoint.Middleware;
using Steeltoe.Management.Endpoint.Security;
using System.Threading.Tasks;

namespace Steeltoe.Management.Endpoint.Health;

public class HealthEndpointMiddleware : EndpointMiddleware<HealthEndpointResponse, ISecurityContext>
{
    private readonly RequestDelegate _next;

    public HealthEndpointMiddleware(RequestDelegate next, IManagementOptions managementOptions, ILogger<InfoEndpointMiddleware> logger = null)
        : base(managementOptions: managementOptions, logger: logger)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context, HealthEndpointCore endpoint)
    {
        base.endpoint = endpoint;
        if (base.endpoint.ShouldInvoke(managementOptions))
        {
            return HandleHealthRequestAsync(context);
        }

        return Task.CompletedTask;
    }

    protected internal Task HandleHealthRequestAsync(HttpContext context)
    {
        var serialInfo = DoRequest(context);
        logger?.LogDebug("Returning: {0}", serialInfo);

        context.HandleContentNegotiation(logger);
        return context.Response.WriteAsync(serialInfo);
    }

    protected internal string DoRequest(HttpContext context)
    {
        var result = endpoint.Invoke(new CoreSecurityContext(context));
        if (managementOptions.UseStatusCodeFromResponse)
        {
            context.Response.StatusCode = ((HealthEndpoint)endpoint).GetStatusCode(result);
        }

        return Serialize(result);
    }
}
