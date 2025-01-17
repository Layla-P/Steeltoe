// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Steeltoe.Management.Endpoint.Metrics;

public class MetricsObserverOptions : IMetricsObserverOptions
{
    internal const string ManagementMetricsPrefix = "management:metrics:observer";
    internal const string DefaultIngressIgnorePattern = "/cloudfoundryapplication|/cloudfoundryapplication/.*|.*\\.png|.*\\.css|.*\\.js|.*\\.html|/favicon.ico|/hystrix.stream|.*\\.gif";
    internal const string DefaultEgressIgnorePattern = "/api/v2/spans|/v2/apps/.*/permissions";

    public MetricsObserverOptions()
    {
        IngressIgnorePattern = DefaultIngressIgnorePattern;
        EgressIgnorePattern = DefaultEgressIgnorePattern;
    }

    public MetricsObserverOptions(IConfiguration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var section = config.GetSection(ManagementMetricsPrefix);
        if (section != null)
        {
            section.Bind(this);
        }

        if (string.IsNullOrEmpty(IngressIgnorePattern))
        {
            IngressIgnorePattern = DefaultIngressIgnorePattern;
        }

        if (string.IsNullOrEmpty(EgressIgnorePattern))
        {
            EgressIgnorePattern = DefaultEgressIgnorePattern;
        }
    }

    /// <inheritdoc/>
    public string IngressIgnorePattern { get; set; }

    /// <inheritdoc/>
    public string EgressIgnorePattern { get; set; }

    public bool AspNetCoreHosting { get; set; } = true;

    public bool GCEvents { get; set; } = true;

    public bool ThreadPoolEvents { get; set; } = true;

    public bool EventCounterEvents { get; set; }

    public bool HttpClientCore { get; set; }

    public bool HttpClientDesktop { get; set; }

    public bool HystrixEvents { get; set; }

    /// <inheritdoc/>
    public List<string> ExcludedMetrics { get; set; } = new ();
}
