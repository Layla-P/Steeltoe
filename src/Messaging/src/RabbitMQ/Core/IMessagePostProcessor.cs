// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.Messaging.RabbitMQ.Connection;

namespace Steeltoe.Messaging.RabbitMQ.Core;

public interface IMessagePostProcessor : Messaging.Core.IMessagePostProcessor
{
    IMessage PostProcessMessage(IMessage message, CorrelationData correlation);
}
