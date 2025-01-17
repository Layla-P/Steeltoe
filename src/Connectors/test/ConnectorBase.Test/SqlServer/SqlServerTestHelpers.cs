// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

namespace Steeltoe.Connector.SqlServer.Test;

public static class SqlServerTestHelpers
{
    public static string SingleServerVcap = @"
            {
                ""SqlServer"": [
                    {
                        ""credentials"": {
                            ""uid"": ""uf33b2b30783a4087948c30f6c3b0c90f"",
                            ""uri"": ""jdbc:sqlserver://192.168.0.80:1433;databaseName=de5aa3a747c134b3d8780f8cc80be519e"",
                            ""db"": ""de5aa3a747c134b3d8780f8cc80be519e"",
                            ""pw"": ""Pefbb929c1e0945b5bab5b8f0d110c503""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""SqlServer"",
                        ""provider"": null,
                        ""plan"": ""sharedVM"",
                        ""name"": ""mySqlServerService"",
                        ""tags"": [
                            ""sqlserver""
                        ]
                    },
                ]
            }";

    public static string SingleServerVcapNoTag = @"
            {
                ""SqlServer"": [
                    {
                        ""credentials"": {
                            ""uid"": ""uf33b2b30783a4087948c30f6c3b0c90f"",
                            ""uri"": ""sqlserver://192.168.0.80:1433;databaseName=de5aa3a747c134b3d8780f8cc80be519e"",
                            ""db"": ""de5aa3a747c134b3d8780f8cc80be519e"",
                            ""pw"": ""Pefbb929c1e0945b5bab5b8f0d110c503""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""SqlServer"",
                        ""provider"": null,
                        ""plan"": ""sharedVM"",
                        ""name"": ""mySqlServerService"",
                        ""tags"": [
                        ]
                    },
                ]
            }";

    public static string SingleServerVcapIgnoreName = @"
            {
                ""user-provided"": [{
                        ""credentials"": {
                            ""uid"": ""uf33b2b30783a4087948c30f6c3b0c90f"",
                            ""uri"": ""sqlserver://192.168.0.80:1433;databaseName=de5aa3a747c134b3d8780f8cc80be519e"",
                            ""db"": ""de5aa3a747c134b3d8780f8cc80be519e"",
                            ""pw"": ""Pefbb929c1e0945b5bab5b8f0d110c503""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""SqlServer"",
                        ""provider"": null,
                        ""plan"": ""sharedVM"",
                        ""name"": ""mySqlServerService"",
                        ""tags"": [""sqlserver""]
                    },
                ]
            }";

    public static string SingleServerVcapCredentialsInUrl = @"
            {
                ""SqlServer"": [{
                        ""credentials"": {
                            ""uri"": ""jdbc:sqlserver://uf33b2b30783a4087948c30f6c3b0c90f:Pefbb929c1e0945b5bab5b8f0d110c503@192.168.0.80:1433;databaseName=de5aa3a747c134b3d8780f8cc80be519e"",
                            ""db"": ""de5aa3a747c134b3d8780f8cc80be519e""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""SqlServer"",
                        ""provider"": null,
                        ""plan"": ""sharedVM"",
                        ""name"": ""mySqlServerService"",
                        ""tags"": [""sqlserver""]
                    },
                ]
            }";

    public static string TwoServerVcap = @"
            {
                ""SqlServer"": [{
                        ""credentials"": {
                            ""uid"": ""uf33b2b30783a4087948c30f6c3b0c90f"",
                            ""uri"": ""jdbc:sqlserver://192.168.0.80:1433;databaseName=db1"",
                            ""db"": ""de5aa3a747c134b3d8780f8cc80be519e"",
                            ""pw"": ""Pefbb929c1e0945b5bab5b8f0d110c503""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""SqlServer"",
                        ""provider"": null,
                        ""plan"": ""sharedVM"",
                        ""name"": ""mySqlServerService"",
                        ""tags"": [""sqlserver""]
                    },
                    {
                        ""credentials"": {
                            ""uid"": ""uf33b2b30783a4087948c30f6c3b0c90f"",
                            ""uri"": ""jdbc:sqlserver://192.168.0.80:1433;databaseName=db2"",
                            ""db"": ""de5aa3a747c134b3d8780f8cc80be519e"",
                            ""pw"": ""Pefbb929c1e0945b5bab5b8f0d110c503""
                        },
                        ""syslog_drain_url"": null,
                        ""label"": ""SqlServer"",
                        ""provider"": null,
                        ""plan"": ""sharedVM"",
                        ""name"": ""mySqlServerService"",
                        ""tags"": [""sqlserver""]
                    },
                ]
            }";
}
