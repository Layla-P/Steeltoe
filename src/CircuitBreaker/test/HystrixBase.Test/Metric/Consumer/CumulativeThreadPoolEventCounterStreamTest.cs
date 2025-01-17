// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.CircuitBreaker.Hystrix.Exceptions;
using Steeltoe.CircuitBreaker.Hystrix.Metric.Test;
using Steeltoe.CircuitBreaker.Hystrix.Test;
using Steeltoe.Common.Util;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Steeltoe.CircuitBreaker.Hystrix.Metric.Consumer.Test;

public class CumulativeThreadPoolEventCounterStreamTest : CommandStreamTest
{
    private readonly ITestOutputHelper _output;
    private CumulativeThreadPoolEventCounterStream _stream;
    private IDisposable _latchSubscription;

    private sealed class LatchedObserver : TestObserverBase<long[]>
    {
        public LatchedObserver(ITestOutputHelper output, CountdownEvent latch)
            : base(output, latch)
        {
        }
    }

    public CumulativeThreadPoolEventCounterStreamTest(ITestOutputHelper output)
    {
        _output = output;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _latchSubscription?.Dispose();
            _latchSubscription = null;

            _stream?.Unsubscribe();
            _stream = null;
        }

        base.Dispose(disposing);
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public void TestEmptyStreamProducesZeros()
    {
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-A");

        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);

        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);
        _stream.StartCachingStreamValuesIfUnstarted();
        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestSingleSuccess()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-B");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-B");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-B");
        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);

        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);
        var cmd = Command.From(groupKey, key, HystrixEventType.Success, 0);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        await cmd.Observe();

        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(1, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestSingleFailure()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-C");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-C");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-C");

        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);
        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);

        var cmd = Command.From(groupKey, key, HystrixEventType.Failure, 0);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        await cmd.Observe();

        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(1, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestSingleTimeout()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-D");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-D");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-D");
        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);

        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);
        var cmd = Command.From(groupKey, key, HystrixEventType.Timeout);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        await cmd.Observe();
        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(1, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestSingleBadRequest()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-E");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-E");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-E");
        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);

        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);
        var cmd = Command.From(groupKey, key, HystrixEventType.BadRequest);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        await Assert.ThrowsAsync<HystrixBadRequestException>(async () => await cmd.Observe());

        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(1, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestRequestFromCache()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-F");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-F");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-F");

        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);
        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);

        var cmd1 = Command.From(groupKey, key, HystrixEventType.Success, 0);
        var cmd2 = Command.From(groupKey, key, HystrixEventType.ResponseFromCache);
        var cmd3 = Command.From(groupKey, key, HystrixEventType.ResponseFromCache);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        await cmd1.Observe();
        await cmd2.Observe();
        await cmd3.Observe();

        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        // RESPONSE_FROM_CACHE should not show up at all in thread pool counters - just the success
        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(1, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestShortCircuited()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-G");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-G");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-G");

        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);
        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);

        var failure1 = Command.From(groupKey, key, HystrixEventType.Failure, 0);
        var failure2 = Command.From(groupKey, key, HystrixEventType.Failure, 0);
        var failure3 = Command.From(groupKey, key, HystrixEventType.Failure, 0);

        var shortCircuit1 = Command.From(groupKey, key, HystrixEventType.Success);
        var shortCircuit2 = Command.From(groupKey, key, HystrixEventType.Success);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        // 3 failures in a row will trip circuit.  let bucket roll once then submit 2 requests.
        // should see 3 FAILUREs and 2 SHORT_CIRCUITs and each should see a FALLBACK_SUCCESS
        await failure1.Observe();
        await failure2.Observe();
        await failure3.Observe();

        Assert.True(WaitForHealthCountToUpdate(key.Name, 500, _output), "health count took to long to update");

        _output.WriteLine(Time.CurrentTimeMillis + " running failures");
        await shortCircuit1.Observe();
        await shortCircuit2.Observe();

        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.True(shortCircuit1.IsResponseShortCircuited);
        Assert.True(shortCircuit2.IsResponseShortCircuited);

        // only the FAILUREs should show up in thread pool counters
        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(3, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestSemaphoreRejected()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-H");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-H");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-H");

        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);
        var saturators = new List<Command>();

        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);

        for (var i = 0; i < 10; i++)
        {
            saturators.Add(Command.From(groupKey, key, HystrixEventType.Success, 500, ExecutionIsolationStrategy.Semaphore));
        }

        var rejected1 = Command.From(groupKey, key, HystrixEventType.Success, 0, ExecutionIsolationStrategy.Semaphore);
        var rejected2 = Command.From(groupKey, key, HystrixEventType.Success, 0, ExecutionIsolationStrategy.Semaphore);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        // 10 commands will saturate semaphore when called from different threads.
        // submit 2 more requests and they should be SEMAPHORE_REJECTED
        // should see 10 SUCCESSes, 2 SEMAPHORE_REJECTED and 2 FALLBACK_SUCCESSes
        var tasks = new List<Task>();
        foreach (var saturator in saturators)
        {
            tasks.Add(Task.Run(() => saturator.Execute()));
        }

        await Task.Delay(50);

        tasks.Add(Task.Run(() => rejected1.Execute()));
        tasks.Add(Task.Run(() => rejected2.Execute()));

        Task.WaitAll(tasks.ToArray());
        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.True(rejected1.IsResponseSemaphoreRejected);
        Assert.True(rejected2.IsResponseSemaphoreRejected);

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public void TestThreadPoolRejected()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-I");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-I");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-I");

        var saturators = new List<Command>();
        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);

        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);

        for (var i = 0; i < 10; i++)
        {
            saturators.Add(Command.From(groupKey, key, HystrixEventType.Success, 500));
        }

        var rejected1 = Command.From(groupKey, key, HystrixEventType.Success, 0);
        var rejected2 = Command.From(groupKey, key, HystrixEventType.Success, 0);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        // 10 commands will saturate thread-pools when called concurrently.
        // submit 2 more requests and they should be THREADPOOL_REJECTED
        // should see 10 SUCCESSes, 2 THREADPOOL_REJECTED and 2 FALLBACK_SUCCESSes
        var tasks = new List<Task>();
        foreach (var c in saturators)
        {
            tasks.Add(c.ExecuteAsync());
        }

        Time.Wait(50);

        tasks.Add(rejected1.ExecuteAsync());
        tasks.Add(rejected2.ExecuteAsync());

        Task.WaitAll(tasks.ToArray());
        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.True(rejected1.IsResponseThreadPoolRejected);
        Assert.True(rejected2.IsResponseThreadPoolRejected);

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(10, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(2, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestFallbackFailure()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-J");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-J");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-J");

        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);
        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);

        var cmd = Command.From(groupKey, key, HystrixEventType.Failure, 0, HystrixEventType.FallbackFailure);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        await Assert.ThrowsAsync<HystrixRuntimeException>(async () => await cmd.Observe());
        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(1, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestFallbackMissing()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-K");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-K");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-K");

        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);
        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);

        var cmd = Command.From(groupKey, key, HystrixEventType.Failure, 0, HystrixEventType.FallbackMissing);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        await Assert.ThrowsAsync<HystrixRuntimeException>(async () => await cmd.Observe());

        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(1, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestFallbackRejection()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-L");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-L");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-L");

        var fallbackSaturators = new List<Command>();
        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);

        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);
        for (var i = 0; i < 5; i++)
        {
            fallbackSaturators.Add(Command.From(groupKey, key, HystrixEventType.Failure, 0, HystrixEventType.FallbackSuccess, 500));
        }

        var rejection1 = Command.From(groupKey, key, HystrixEventType.Failure, 0, HystrixEventType.FallbackSuccess, 0);
        var rejection2 = Command.From(groupKey, key, HystrixEventType.Failure, 0, HystrixEventType.FallbackSuccess, 0);

        _latchSubscription = _stream.Observe().Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        // fallback semaphore size is 5.  So let 5 commands saturate that semaphore, then
        // let 2 more commands go to fallback.  they should get rejected by the fallback-semaphore
        var tasks = new List<Task>();
        foreach (var saturator in fallbackSaturators)
        {
            tasks.Add(saturator.ExecuteAsync());
        }

        await Task.Delay(50);

        _output.WriteLine("ReqLog : " + HystrixRequestLog.CurrentRequestLog.GetExecutedCommandsAsString());

        await Assert.ThrowsAsync<HystrixRuntimeException>(async () => await rejection1.Observe());
        await Assert.ThrowsAsync<HystrixRuntimeException>(async () => await rejection2.Observe());

        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        _output.WriteLine("ReqLog : " + HystrixRequestLog.CurrentRequestLog.GetExecutedCommandsAsString());

        Task.WaitAll(tasks.ToArray());
        Assert.True(WaitForLatchedObserverToUpdate(observer, 1, 500, _output), "Latch took to long to update");

        // all 7 commands executed on-thread, so should be executed according to thread-pool metrics
        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(7, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }

    // in a rolling window, take(20) would age out all counters.  in the cumulative count, we expect them to remain non-zero forever
    [Fact]
    [Trait("Category", "FlakyOnHostedAgents")]
    public async Task TestMultipleEventsOverTimeGetStoredAndDoNotAgeOut()
    {
        var groupKey = HystrixCommandGroupKeyDefault.AsKey("Cumulative-ThreadPool-M");
        var threadPoolKey = HystrixThreadPoolKeyDefault.AsKey("Cumulative-ThreadPool-M");
        var key = HystrixCommandKeyDefault.AsKey("Cumulative-Counter-M");
        var latch = new CountdownEvent(1);
        var observer = new LatchedObserver(_output, latch);

        _stream = CumulativeThreadPoolEventCounterStream.GetInstance(threadPoolKey, 10, 100);
        var cmd1 = Command.From(groupKey, key, HystrixEventType.Success, 20);
        var cmd2 = Command.From(groupKey, key, HystrixEventType.Failure, 10);

        _latchSubscription = _stream.Observe().Take(20 + LatchedObserver.StableTickCount).Subscribe(observer);
        Assert.True(Time.WaitUntil(() => observer.StreamRunning, 1000), "Stream failed to start");

        await cmd1.Observe();
        await cmd2.Observe();
        Assert.True(latch.Wait(20000), "CountdownEvent was not set!");

        _output.WriteLine("ReqLog : " + HystrixRequestLog.CurrentRequestLog.GetExecutedCommandsAsString());

        // all commands should not have aged out
        Assert.Equal(2, _stream.Latest.Length);
        Assert.Equal(2, _stream.GetLatestCount(ThreadPoolEventType.Executed));
        Assert.Equal(0, _stream.GetLatestCount(ThreadPoolEventType.Rejected));
    }
}
