// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Steeltoe.Connector.Oracle;

public class OracleProviderConnectorOptions : AbstractServiceConnectorOptions
{
    public const string DefaultServer = "localhost";
    public const int DefaultPort = 1521;
    private const string OracleClientSectionPrefix = "oracle:client";
    private readonly bool _cloudFoundryConfigFound;

    public OracleProviderConnectorOptions()
    {
    }

    public OracleProviderConnectorOptions(IConfiguration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var section = config.GetSection(OracleClientSectionPrefix);

        section.Bind(this);

        _cloudFoundryConfigFound = config.HasCloudFoundryServiceConfigurations();
    }

    public string ConnectionString { get; set; }

    public string Server { get; set; } = DefaultServer;

    public int Port { get; set; } = DefaultPort;

    public string Username { get; set; }

    public string Password { get; set; }

    public string ServiceName { get; set; }

    public int ConnectionTimeout { get; set; } = 15;

    internal Dictionary<string, string> Options { get; set; } = new ();

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(ConnectionString) && !_cloudFoundryConfigFound)
        {
            return ConnectionString;
        }

        return
            $"User Id={Username};Password={Password};Data Source={Server}:{Port}/{ServiceName};Connection Timeout={ConnectionTimeout}";
    }
}
