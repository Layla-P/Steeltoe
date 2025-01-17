// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.Management.Endpoint.CloudFoundry;
using System.Collections.Generic;

namespace Steeltoe.Management.Endpoint.Test;

public static class ManagementTestHelpers
{
    public static IEnumerable<IManagementOptions> GetManagementOptions(params IEndpointOptions[] options)
    {
        var managementOptions = new CloudFoundryManagementOptions();
        managementOptions.EndpointOptions.AddRange(options);
        return new List<IManagementOptions> { managementOptions };
    }
}
