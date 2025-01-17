// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Serialization;

namespace Steeltoe.CircuitBreaker.Hystrix.Exceptions;

[Serializable]
public class HystrixBadRequestException : Exception
{
    public HystrixBadRequestException(string message)
        : base(message)
    {
    }

    public HystrixBadRequestException(string message, Exception cause)
        : base(message, cause)
    {
    }

    public HystrixBadRequestException()
    {
    }

    protected HystrixBadRequestException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
