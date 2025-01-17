// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Logging;
using Steeltoe.Messaging.RabbitMQ.Connection;
using Steeltoe.Messaging.RabbitMQ.Core;
using Steeltoe.Messaging.RabbitMQ.Extensions;
using Steeltoe.Messaging.RabbitMQ.Support;
using System;
using System.Collections.Generic;

namespace Steeltoe.Messaging.RabbitMQ.Retry;

public class RepublishMessageRecoverer : IMessageRecoverer
{
    public const string XExceptionStacktrace = "x-exception-stacktrace";
    public const string XExceptionMessage = "x-exception-message";
    public const string XOriginalExchange = "x-original-exchange";
    public const string XOriginalRoutingKey = "x-original-routingKey";
    public const int DefaultFrameMaxHeadroom = 20_000;

    private const int EllipsisLength = 3;
    private const int MaxExceptionMessageSizeInTrace = 100 - EllipsisLength;

    private readonly ILogger _logger;

    public RepublishMessageRecoverer(RabbitTemplate errorTemplate, ILogger logger = null)
        : this(errorTemplate, null, null, logger)
    {
    }

    public RepublishMessageRecoverer(RabbitTemplate errorTemplate, string errorExchange, ILogger logger = null)
        : this(errorTemplate, errorExchange, null, logger)
    {
    }

    public RepublishMessageRecoverer(RabbitTemplate errorTemplate, string errorExchange, string errorRoutingKey, ILogger logger = null)
    {
        ErrorTemplate = errorTemplate ?? throw new ArgumentNullException(nameof(errorTemplate));
        ErrorExchangeName = errorExchange;
        ErrorRoutingKey = errorRoutingKey;
        MaxStackTraceLength = int.MaxValue;
        _logger = logger;
    }

    public RabbitTemplate ErrorTemplate { get; }

    public string ErrorExchangeName { get; }

    public string ErrorRoutingKey { get; }

    public int MaxStackTraceLength { get; set; }

    public string ErrorRoutingKeyPrefix { get; set; }

    public MessageDeliveryMode DeliveryMode { get; set; }

    public int FrameMaxHeadroom { get; set; } = DefaultFrameMaxHeadroom;

    public void Recover(IMessage message, Exception exception)
    {
        var headers = RabbitHeaderAccessor.GetMutableAccessor(message);

        var exceptionMessage = exception.InnerException != null ? exception.InnerException.Message : exception.Message;
        var processed = ProcessStackTrace(exception, exceptionMessage);
        var stackTraceAsString = processed[0];
        var truncatedExceptionMessage = processed[1];
        if (truncatedExceptionMessage != null)
        {
            exceptionMessage = truncatedExceptionMessage;
        }

        headers.SetHeader(XExceptionStacktrace, stackTraceAsString);
        headers.SetHeader(XExceptionMessage, exceptionMessage);
        headers.SetHeader(XOriginalExchange, headers.ReceivedExchange);
        headers.SetHeader(XOriginalRoutingKey, headers.ReceivedRoutingKey);
        var additionalHeaders = AddAdditionalHeaders(message, exception);

        if (additionalHeaders != null)
        {
            foreach (var h in additionalHeaders)
            {
                headers.SetHeader(h.Key, h.Value);
            }
        }

        headers.DeliveryMode ??= DeliveryMode;

        if (ErrorExchangeName != null)
        {
            var routingKey = ErrorRoutingKey ?? PrefixedOriginalRoutingKey(message);
            ErrorTemplate.Send(ErrorExchangeName, routingKey, message);

            _logger?.LogWarning("Republishing failed message to exchange '{exchange}' with routing key '{routingKey}'", ErrorExchangeName, routingKey);
        }
        else
        {
            var routingKey = PrefixedOriginalRoutingKey(message);
            ErrorTemplate.Send(routingKey, message);

            _logger?.LogWarning("Republishing failed message to the template's default exchange with routing key {key}", routingKey);
        }
    }

    protected virtual Dictionary<string, object> AddAdditionalHeaders(IMessage message, Exception cause) => null;

    private string PrefixedOriginalRoutingKey(IMessage message) => ErrorRoutingKeyPrefix + message.Headers.ReceivedRoutingKey();

    private List<string> ProcessStackTrace(Exception cause, string exceptionMessage)
    {
        var stackTraceAsString = cause.StackTrace;
        if (MaxStackTraceLength < 0)
        {
            var maxStackTraceLen = RabbitUtils.GetMaxFrame(ErrorTemplate.ConnectionFactory);
            if (maxStackTraceLen > 0)
            {
                maxStackTraceLen -= FrameMaxHeadroom;
                MaxStackTraceLength = maxStackTraceLen;
            }
        }

        return TruncateIfNecessary(cause, exceptionMessage, stackTraceAsString);
    }

    private List<string> TruncateIfNecessary(Exception cause, string exception, string stackTrace)
    {
        var truncated = false;
        var stackTraceAsString = stackTrace;
        var exceptionMessage = exception;
        var truncatedExceptionMessage = exceptionMessage.Length <= MaxExceptionMessageSizeInTrace ? exceptionMessage : exceptionMessage.Substring(0, MaxExceptionMessageSizeInTrace) + "...";
        if (MaxStackTraceLength > 0 && stackTraceAsString.Length + exceptionMessage.Length > MaxStackTraceLength)
        {
            if (!exceptionMessage.Equals(truncatedExceptionMessage))
            {
                var start = stackTraceAsString.IndexOf(exceptionMessage);
                stackTraceAsString = stackTraceAsString.Substring(0, start)
                                     + truncatedExceptionMessage
                                     + stackTraceAsString.Substring(start + exceptionMessage.Length);
            }

            var adjustedStackTraceLen = MaxStackTraceLength - truncatedExceptionMessage.Length;
            if (adjustedStackTraceLen > 0)
            {
                if (stackTraceAsString.Length > adjustedStackTraceLen)
                {
                    stackTraceAsString = stackTraceAsString.Substring(0, adjustedStackTraceLen);
                    _logger?.LogWarning(cause, "Stack trace in republished message header truncated due to frame_max limitations; consider increasing frame_max on the broker or reduce the stack trace depth");
                    truncated = true;
                }
                else if (stackTraceAsString.Length + exceptionMessage.Length > MaxStackTraceLength)
                {
                    _logger?.LogWarning(cause, "Exception message in republished message header truncated due to frame_max limitations; consider increasing frame_max on the broker or reduce the exception message size");
                    truncatedExceptionMessage = $"{exceptionMessage.Substring(0, MaxStackTraceLength - stackTraceAsString.Length - EllipsisLength)}...";
                    truncated = true;
                }
            }
        }

        return new List<string> { stackTraceAsString, truncated ? truncatedExceptionMessage : null };
    }
}
