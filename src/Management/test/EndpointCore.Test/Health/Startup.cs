// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Steeltoe.Management.Endpoint.Health.Contributor;

namespace Steeltoe.Management.Endpoint.Health.Test;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();
        switch (Configuration.GetValue<string>("HealthCheckType"))
        {
            case "down":
                services.AddHealthActuator(Configuration, typeof(DownContributor));
                break;
            case "out":
                services.AddHealthActuator(Configuration, typeof(OutOfServiceContributor));
                break;
            case "unknown":
                services.AddHealthActuator(Configuration, typeof(UnknownContributor));
                break;
            case "defaultAggregator":
                services.AddHealthActuator(Configuration, new DefaultHealthAggregator(), typeof(DiskSpaceContributor));
                break;
            case "microsoftHealthAggregator":
                services.AddSingleton<IOptionsMonitor<HealthCheckServiceOptions>>(new TestServiceOptions());
                services.AddHealthActuator(Configuration, new HealthRegistrationsAggregator(), typeof(DiskSpaceContributor));
                break;
            default:
                services.AddHealthActuator(Configuration);
                break;
        }
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.Map<HealthEndpointCore>();
        });
    }
}