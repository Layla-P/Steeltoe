// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Builder;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.CircuitBreaker.Hystrix.Test;

public class HystrixApplicationBuilderExtensionsTest
{
    private readonly ITestOutputHelper _output;

    public HystrixApplicationBuilderExtensionsTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void UseHystrixRequestContext_ThrowsIfBuilderNull()
    {
        const IApplicationBuilder builder = null;

        var ex = Assert.Throws<ArgumentNullException>(() => builder.UseHystrixRequestContext());
        Assert.Contains(nameof(builder), ex.Message);
    }
}
