// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Steeltoe.Connector.PostgreSql;

public class PostgresProviderConnectorOptions : AbstractServiceConnectorOptions
{
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 5432;
    private const string PostgresClientSectionPrefix = "postgres:client";
    private readonly bool _cloudFoundryConfigFound;

    public PostgresProviderConnectorOptions()
    {
    }

    public PostgresProviderConnectorOptions(IConfiguration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        var section = config.GetSection(PostgresClientSectionPrefix);
        section.Bind(this);

        _cloudFoundryConfigFound = config.HasCloudFoundryServiceConfigurations();
    }

    public string ConnectionString { get; set; }

    public string Host { get; set; } = DefaultHost;

    public int Port { get; set; } = DefaultPort;

    public string Username { get; set; }

    public string Password { get; set; }

    public string Database { get; set; }

    public string SearchPath { get; set; }

    public string SslMode { get; set; }

    public string ClientCertificate { get; set; }

    public string ClientKey { get; set; }

    public string SslRootCertificate { get; set; }

    public int Timeout { get; set; } = 15;

    public int CommandTimeout { get; set; } = 30;

    public bool? TrustServerCertificate { get; set; } = null;

    internal Dictionary<string, string> Options { get; set; } = new ();

    public override string ToString()
    {
        StringBuilder sb;

        if (!string.IsNullOrEmpty(ConnectionString) && !_cloudFoundryConfigFound)
        {
            sb = new StringBuilder(ConnectionString);
        }
        else
        {
            sb = new StringBuilder();
            AddKeyValue(sb, nameof(Host), Host);
            AddKeyValue(sb, nameof(Port), Port);
            AddKeyValue(sb, nameof(Username), Username);
            AddKeyValue(sb, nameof(Password), Password);
            AddKeyValue(sb, nameof(Database), Database);
        }

        AddKeyValue(sb, nameof(Timeout), Timeout);
        AddKeyValue(sb, "Command Timeout", CommandTimeout);
        AddKeyValue(sb, "Search Path", SearchPath);
        AddKeyValue(sb, "sslmode", SslMode);
        AddKeyValue(sb, "Trust Server Certificate", TrustServerCertificate);

        if (Options != null && Options.Any())
        {
            foreach (var o in Options)
            {
                AddKeyValue(sb, o.Key, o.Value);
            }
        }

        return sb.ToString();
    }
}
