// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Logging;
using Steeltoe.Common.Discovery;
using Steeltoe.Common.LoadBalancer;
using Steeltoe.Discovery;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Steeltoe.Common.Http.Discovery;

/// <summary>
/// A <see cref="DelegatingHandler"/> implementation that performs Service Discovery.
/// </summary>
public class DiscoveryHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger _logger;
    private readonly DiscoveryHttpClientHandlerBase _discoveryBase;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscoveryHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="discoveryClient">Service discovery client to use - provided by calling services.AddDiscoveryClient(Configuration).</param>
    /// <param name="logger">ILogger for capturing logs from Discovery operations.</param>
    /// <param name="loadBalancer">The load balancer to use.</param>
    public DiscoveryHttpMessageHandler(IDiscoveryClient discoveryClient, ILogger<DiscoveryHttpClientHandler> logger = null, ILoadBalancer loadBalancer = null)
    {
        _discoveryBase = new DiscoveryHttpClientHandlerBase(discoveryClient, logger, loadBalancer);
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var current = request.RequestUri;
        try
        {
            request.RequestUri = await _discoveryBase.LookupServiceAsync(current).ConfigureAwait(false);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger?.LogDebug(e, "Exception during SendAsync()");
            throw;
        }
        finally
        {
            request.RequestUri = current;
        }
    }
}
