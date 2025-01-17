// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

namespace Steeltoe.Discovery.Eureka;

public class EurekaClientConfig : IEurekaClientConfig
{
    public const int DefaultRegistryFetchIntervalSeconds = 30;
    public const int DefaultInstanceInfoReplicationIntervalSeconds = 40;
    public const int DefaultEurekaServerConnectTimeoutSeconds = 5;
    public const int DefaultEurekaServerRetryCount = 3;
    public const string DefaultServerServiceUrl = "http://localhost:8761/eureka/";

    public EurekaClientConfig()
    {
        RegistryFetchIntervalSeconds = DefaultRegistryFetchIntervalSeconds;
        ShouldGZipContent = true;
        EurekaServerConnectTimeoutSeconds = DefaultEurekaServerConnectTimeoutSeconds;
        ShouldRegisterWithEureka = true;
        ShouldDisableDelta = false;
        ShouldFilterOnlyUpInstances = true;
        ShouldFetchRegistry = true;
        ShouldOnDemandUpdateStatusChange = true;
        EurekaServerServiceUrls = DefaultServerServiceUrl;
        ValidateCertificates = true;
        EurekaServerRetryCount = DefaultEurekaServerRetryCount;
        HealthCheckEnabled = true;
        HealthContribEnabled = true;
    }

    // Configuration property: eureka:client:registryFetchIntervalSeconds
    public int RegistryFetchIntervalSeconds { get; set; }

    // Configuration property: eureka:client:shouldRegisterWithEureka
    public bool ShouldRegisterWithEureka { get; set; }

    // Configuration property: eureka:client:shouldDisableDelta
    public bool ShouldDisableDelta { get; set; }

    // Configuration property: eureka:client:shouldFilterOnlyUpInstances
    public bool ShouldFilterOnlyUpInstances { get; set; }

    // Configuration property: eureka:client:shouldFetchRegistry
    public bool ShouldFetchRegistry { get; set; }

    // Configuration property: eureka:client:registryRefreshSingleVipAddress
    public string RegistryRefreshSingleVipAddress { get; set; }

    // Configuration property: eureka:client:shouldOnDemandUpdateStatusChange
    public bool ShouldOnDemandUpdateStatusChange { get; set; }

    // Configuration property: eureka:client:enabled
    public bool Enabled { get; set; } = true;

    // Configuration property: eureka:client:healthCheckEnabled
    public bool HealthCheckEnabled { get; set; }

    public string EurekaServerServiceUrls { get; set; }

    public int EurekaServerConnectTimeoutSeconds { get; set; }

    public int EurekaServerRetryCount { get; set; }

    public string ProxyHost { get; set; }

    public int ProxyPort { get; set; }

    public string ProxyUserName { get; set; }

    public string ProxyPassword { get; set; }

    public bool ShouldGZipContent { get; set; }

    public bool ValidateCertificates { get; set; }

    public bool HealthContribEnabled { get; set; }

    public string HealthMonitoredApps { get; set; }
}
