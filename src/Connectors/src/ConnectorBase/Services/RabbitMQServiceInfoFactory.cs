// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.Extensions.Configuration;

namespace Steeltoe.Connector.Services;

public class RabbitMQServiceInfoFactory : ServiceInfoFactory
{
    public static readonly Tags RabbitServiceTags = new ("rabbit");

    private static readonly string[] Scheme = { RabbitMQServiceInfo.AmqpScheme, RabbitMQServiceInfo.AmqpSecureScheme };

    public RabbitMQServiceInfoFactory()
        : base(RabbitServiceTags, Scheme)
    {
    }

    public override bool Accepts(Service binding)
    {
        var result = base.Accepts(binding);
        if (result)
        {
            result = !HystrixRabbitMQServiceInfoFactory.HystrixRabbitServiceTags.ContainsOne(binding.Tags);
        }

        return result;
    }

    public override IServiceInfo Create(Service binding)
    {
        var uri = GetUriFromCredentials(binding.Credentials);
        var managementUri = GetStringFromCredentials(binding.Credentials, "http_api_uri");

        if (binding.Credentials.ContainsKey("uris"))
        {
            var uris = GetListFromCredentials(binding.Credentials, "uris");
            var managementUris = GetListFromCredentials(binding.Credentials, "http_api_uris");
            return new RabbitMQServiceInfo(binding.Name, uri, managementUri, uris, managementUris);
        }

        return new RabbitMQServiceInfo(binding.Name, uri, managementUri);
    }
}
