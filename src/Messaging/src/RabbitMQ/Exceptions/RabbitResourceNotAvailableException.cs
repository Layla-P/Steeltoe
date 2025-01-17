// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;

namespace Steeltoe.Messaging.RabbitMQ.Exceptions;

public class RabbitResourceNotAvailableException : RabbitException
{
    public RabbitResourceNotAvailableException(string message)
        : base(message)
    {
    }

    public RabbitResourceNotAvailableException(Exception cause)
        : base(cause)
    {
    }

    public RabbitResourceNotAvailableException(string message, Exception cause)
        : base(message, cause)
    {
    }
}
