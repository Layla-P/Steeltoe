// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.CircuitBreaker.Hystrix.Metric;
using Steeltoe.CircuitBreaker.Hystrix.Strategy.Concurrency;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steeltoe.Common.Util;

namespace Steeltoe.CircuitBreaker.Hystrix;

public class HystrixRequestLog
{
    private sealed class HystrixRequestLogVariable : HystrixRequestVariableDefault<HystrixRequestLog>
    {
        public HystrixRequestLogVariable()
            : base(() => new HystrixRequestLog(), log =>
            {
                HystrixRequestEventsStream.GetInstance().Write(log.AllExecutedCommands);
            })
        {
        }
    }

    private static readonly HystrixRequestLogVariable RequestLog = new ();

    public static HystrixRequestLog CurrentRequestLog
    {
        get
        {
            if (HystrixRequestContext.IsCurrentThreadInitialized)
            {
                return RequestLog.Value;
            }
            else
            {
                return null;
            }
        }
    }

    protected internal const int MaxStorage = 1000;
    private readonly BlockingCollection<IHystrixInvokableInfo> _allExecutedCommands = new (MaxStorage);

    internal HystrixRequestLog()
    {
    }

    internal void AddExecutedCommand(IHystrixInvokableInfo command)
    {
        if (!_allExecutedCommands.TryAdd(command))
        {
            // see RequestLog: Reduce Chance of Memory Leak https://github.com/Netflix/Hystrix/issues/53
            // logger.warn("RequestLog ignoring command after reaching limit of " + MAX_STORAGE + ". See https://github.com/Netflix/Hystrix/issues/53 for more information.");
        }
    }

    public ICollection<IHystrixInvokableInfo> AllExecutedCommands
    {
        get
        {
            return _allExecutedCommands.ToList().AsReadOnly();
        }
    }

    public string GetExecutedCommandsAsString()
    {
        try
        {
            var aggregatedCommandsExecuted = new Dictionary<string, int>();
            var aggregatedCommandExecutionTime = new Dictionary<string, int>();

            var builder = new StringBuilder();
            var estimatedLength = 0;
            foreach (var command in _allExecutedCommands)
            {
                builder.Length = 0;
                builder.Append(command.CommandKey.Name);

                var events = new List<HystrixEventType>(command.ExecutionEvents);
                if (events.Count > 0)
                {
                    events.Sort();

                    // replicate functionality of Arrays.toString(events.toArray()) to append directly to existing StringBuilder
                    builder.Append('[');
                    foreach (var ev in events)
                    {
                        var eventName = ev.ToSnakeCaseString(SnakeCaseStyle.AllCaps);

                        switch (ev)
                        {
                            case HystrixEventType.Emit:
                                var numEmissions = command.NumberEmissions;
                                if (numEmissions > 1)
                                {
                                    builder.Append(eventName).Append('x').Append(numEmissions).Append(", ");
                                }
                                else
                                {
                                    builder.Append(eventName).Append(", ");
                                }

                                break;
                            case HystrixEventType.FallbackEmit:
                                var numFallbackEmissions = command.NumberFallbackEmissions;
                                if (numFallbackEmissions > 1)
                                {
                                    builder.Append(eventName).Append('x').Append(numFallbackEmissions).Append(", ");
                                }
                                else
                                {
                                    builder.Append(eventName).Append(", ");
                                }

                                break;
                            default:
                                builder.Append(eventName).Append(", ");
                                break;
                        }
                    }

                    builder[builder.Length - 2] = ']';
                    builder.Length -= 1;
                }
                else
                {
                    builder.Append("[Executed]");
                }

                var display = builder.ToString();
                estimatedLength += display.Length + 12; // add 12 chars to display length for appending totalExecutionTime and count below
                if (aggregatedCommandsExecuted.TryGetValue(display, out var counter))
                {
                    aggregatedCommandsExecuted[display] = counter + 1;
                }
                else
                {
                    // add it
                    aggregatedCommandsExecuted.Add(display, 1);
                }

                var executionTime = command.ExecutionTimeInMilliseconds;
                if (executionTime < 0)
                {
                    // do this so we don't create negative values or subtract values
                    executionTime = 0;
                }

                if (aggregatedCommandExecutionTime.TryGetValue(display, out counter))
                {
                    // add to the existing executionTime (sum of executionTimes for duplicate command displayNames)
                    aggregatedCommandExecutionTime[display] = aggregatedCommandExecutionTime[display] + executionTime;
                }
                else
                {
                    // add it
                    aggregatedCommandExecutionTime.Add(display, executionTime);
                }
            }

            builder.Length = 0;
            builder.EnsureCapacity(estimatedLength);
            foreach (var displayString in aggregatedCommandsExecuted.Keys)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(displayString);
                var totalExecutionTime = aggregatedCommandExecutionTime[displayString];
                builder.Append('[').Append(totalExecutionTime).Append("ms]");

                var count = aggregatedCommandsExecuted[displayString];
                if (count > 1)
                {
                    builder.Append('x').Append(count);
                }
            }

            return builder.ToString();
        }
        catch (Exception)
        {
            // logger.error("Failed to create HystrixRequestLog response header string.", e)
            // don't let this cause the entire app to fail so just return "Unknown"
            return "Unknown";
        }
    }
}
