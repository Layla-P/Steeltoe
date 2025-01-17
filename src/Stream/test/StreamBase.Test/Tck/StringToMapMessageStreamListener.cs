// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.Messaging;
using Steeltoe.Messaging.Handler.Attributes;
using Steeltoe.Stream.Attributes;
using Steeltoe.Stream.Messaging;
using System.Collections.Generic;
using Xunit;

namespace Steeltoe.Stream.Tck;

public class StringToMapMessageStreamListener
{
    [StreamListener(ISink.InputName)]
    [SendTo(ISource.OutputName)]
    public string Echo(IMessage<Dictionary<object, object>> value)
    {
        Assert.IsType<Dictionary<object, object>>(value.Payload);
        value.Payload.TryGetValue("name", out var result);
        return (string)result;
    }
}
