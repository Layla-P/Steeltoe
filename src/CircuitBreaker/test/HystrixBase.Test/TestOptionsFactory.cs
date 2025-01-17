// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.CircuitBreaker.Hystrix.Strategy.Options;
using System;

namespace Steeltoe.CircuitBreaker.Hystrix.Test;

internal sealed class TestOptionsFactory : HystrixOptionsStrategy
{
    public override IHystrixCommandOptions GetCommandOptions(IHystrixCommandKey commandKey, IHystrixCommandOptions builder)
    {
        return builder ?? HystrixCommandOptionsTest.GetUnitTestOptions();
    }

    public override IHystrixThreadPoolOptions GetThreadPoolOptions(IHystrixThreadPoolKey threadPoolKey, IHystrixThreadPoolOptions builder)
    {
        return builder ?? HystrixThreadPoolOptionsTest.GetUnitTestPropertiesBuilder();
    }

    public override IHystrixCollapserOptions GetCollapserOptions(IHystrixCollapserKey collapserKey, IHystrixCollapserOptions builder)
    {
        throw new InvalidOperationException("not expecting collapser properties");
    }

    public override string GetCommandOptionsCacheKey(IHystrixCommandKey commandKey, IHystrixCommandOptions builder)
    {
        return null;
    }

    public override string GetThreadPoolOptionsCacheKey(IHystrixThreadPoolKey threadPoolKey, IHystrixThreadPoolOptions builder)
    {
        return null;
    }

    public override string GetCollapserOptionsCacheKey(IHystrixCollapserKey collapserKey, IHystrixCollapserOptions builder)
    {
        return null;
    }
}
