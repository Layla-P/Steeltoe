// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.Messaging.Converter;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Steeltoe.Messaging.Core;

public abstract class AbstractMessageReceivingTemplate<TDestination> : AbstractMessageSendingTemplate<TDestination>, IMessageReceivingOperations<TDestination>
{
    private TDestination _defaultReceiveDestination;

    public virtual TDestination DefaultReceiveDestination
    {
        get
        {
            return _defaultReceiveDestination;
        }

        set
        {
            _defaultReceiveDestination = value;
        }
    }

    public virtual bool ThrowReceivedExceptions { get; set; }

    public virtual Task<IMessage> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        return DoReceiveAsync(RequiredDefaultReceiveDestination, cancellationToken);
    }

    public virtual Task<IMessage> ReceiveAsync(TDestination destination, CancellationToken cancellationToken = default)
    {
        return DoReceiveAsync(destination, cancellationToken);
    }

    public virtual Task<T> ReceiveAndConvertAsync<T>(CancellationToken cancellationToken = default)
    {
        return ReceiveAndConvertAsync<T>(RequiredDefaultReceiveDestination);
    }

    public virtual async Task<T> ReceiveAndConvertAsync<T>(TDestination destination, CancellationToken cancellationToken = default)
    {
        var message = await DoReceiveAsync(destination, cancellationToken);
        if (message != null)
        {
            return DoConvert<T>(message);
        }
        else
        {
            return default(T);
        }
    }

    public virtual IMessage Receive()
    {
        return DoReceive(RequiredDefaultReceiveDestination);
    }

    public virtual IMessage Receive(TDestination destination)
    {
        return DoReceive(destination);
    }

    public virtual T ReceiveAndConvert<T>()
    {
        return ReceiveAndConvert<T>(RequiredDefaultReceiveDestination);
    }

    public virtual T ReceiveAndConvert<T>(TDestination destination)
    {
        var message = DoReceive(destination);
        if (message != null)
        {
            return DoConvert<T>(message);
        }
        else
        {
            return default;
        }
    }

    protected virtual TDestination RequiredDefaultReceiveDestination
    {
        get
        {
            if (_defaultReceiveDestination == null)
            {
                throw new InvalidOperationException("No default destination configured");
            }

            return _defaultReceiveDestination;
        }
    }

    protected abstract Task<IMessage> DoReceiveAsync(TDestination destination, CancellationToken cancellationToken);

    protected abstract IMessage DoReceive(TDestination destination);

    protected virtual T DoConvert<T>(IMessage message)
    {
        var messageConverter = MessageConverter;
        var value = messageConverter.FromMessage(message, typeof(T));
        if (value == null)
        {
            throw new MessageConversionException(
                message,
                $"Unable to convert payload [{message.Payload}] to type [{typeof(T)}] using converter [{messageConverter}]");
        }

        return value is Exception exception && ThrowReceivedExceptions
            ? throw exception
            : (T)value;
    }
}
