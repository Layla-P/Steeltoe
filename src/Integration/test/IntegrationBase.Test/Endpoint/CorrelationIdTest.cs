// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Common.Contexts;
using Steeltoe.Integration.Channel;
using Steeltoe.Integration.Handler;
using Steeltoe.Integration.Support;
using Steeltoe.Messaging;
using Steeltoe.Messaging.Core;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Steeltoe.Integration.Endpoint.Test;

public class CorrelationIdTest
{
    private readonly IServiceProvider _provider;

    public CorrelationIdTest()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<IApplicationContext, GenericApplicationContext>();
        services.AddSingleton<IDestinationResolver<IMessageChannel>, DefaultMessageChannelDestinationResolver>();
        services.AddSingleton<IMessageBuilderFactory, DefaultMessageBuilderFactory>();
        services.AddSingleton<IIntegrationServices, IntegrationServices>();
        services.AddSingleton<IMessageChannel>(p => new DirectChannel(p.GetService<IApplicationContext>(), "errorChannel"));
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task TestCorrelationIdPassedIfAvailable()
    {
        object correlationId = "123-ABC";
        var message = IntegrationMessageBuilder.WithPayload("test").SetCorrelationId(correlationId).Build();
        var inputChannel = new DirectChannel(_provider.GetService<IApplicationContext>());
        var outputChannel = new QueueChannel(_provider.GetService<IApplicationContext>(), 1);
        var serviceActivator = new ServiceActivatingHandler(_provider.GetService<IApplicationContext>(), new TestBeanUpperCase())
        {
            OutputChannel = outputChannel
        };
        var endpoint = new EventDrivenConsumerEndpoint(_provider.GetService<IApplicationContext>(), inputChannel, serviceActivator);
        await endpoint.Start();
        Assert.True(inputChannel.Send(message));
        var reply = outputChannel.Receive(0);
        var accessor = new IntegrationMessageHeaderAccessor(reply);
        Assert.Equal(correlationId, accessor.GetCorrelationId());
    }

    [Fact]
    public async Task TestCorrelationIdCopiedFromMessageCorrelationIdIfAvailable()
    {
        object correlationId = "correlationId";
        var message = IntegrationMessageBuilder.WithPayload("test").SetCorrelationId(correlationId).Build();
        var inputChannel = new DirectChannel(_provider.GetService<IApplicationContext>());
        var outputChannel = new QueueChannel(_provider.GetService<IApplicationContext>(), 1);
        var serviceActivator = new ServiceActivatingHandler(_provider.GetService<IApplicationContext>(), new TestBeanUpperCase())
        {
            OutputChannel = outputChannel
        };
        var endpoint = new EventDrivenConsumerEndpoint(_provider.GetService<IApplicationContext>(), inputChannel, serviceActivator);
        await endpoint.Start();
        Assert.True(inputChannel.Send(message));
        var reply = outputChannel.Receive(0);
        var accessor1 = new IntegrationMessageHeaderAccessor(reply);
        var accessor2 = new IntegrationMessageHeaderAccessor(message);
        Assert.Equal(accessor2.GetCorrelationId(), accessor1.GetCorrelationId());
    }

    [Fact]
    public async Task TestCorrelationNotPassedFromRequestHeaderIfAlreadySetByHandler()
    {
        object correlationId = "123-ABC";
        var message = IntegrationMessageBuilder.WithPayload("test").SetCorrelationId(correlationId).Build();
        var inputChannel = new DirectChannel(_provider.GetService<IApplicationContext>());
        var outputChannel = new QueueChannel(_provider.GetService<IApplicationContext>(), 1);
        var serviceActivator = new ServiceActivatingHandler(_provider.GetService<IApplicationContext>(), new TestBeanCreateMessage())
        {
            OutputChannel = outputChannel
        };
        var endpoint = new EventDrivenConsumerEndpoint(_provider.GetService<IApplicationContext>(), inputChannel, serviceActivator);
        await endpoint.Start();
        Assert.True(inputChannel.Send(message));
        var reply = outputChannel.Receive(0);
        var accessor = new IntegrationMessageHeaderAccessor(reply);
        Assert.Equal("456-XYZ", accessor.GetCorrelationId());
    }

    [Fact]
    public async Task TestCorrelationNotCopiedFromRequestMessageIdIfAlreadySetByHandler()
    {
        IMessage message = Message.Create("test");
        var inputChannel = new DirectChannel(_provider.GetService<IApplicationContext>());
        var outputChannel = new QueueChannel(_provider.GetService<IApplicationContext>(), 1);
        var serviceActivator = new ServiceActivatingHandler(_provider.GetService<IApplicationContext>(), new TestBeanCreateMessage())
        {
            OutputChannel = outputChannel
        };
        var endpoint = new EventDrivenConsumerEndpoint(_provider.GetService<IApplicationContext>(), inputChannel, serviceActivator);
        await endpoint.Start();
        Assert.True(inputChannel.Send(message));
        var reply = outputChannel.Receive(0);
        var accessor = new IntegrationMessageHeaderAccessor(reply);
        Assert.Equal("456-XYZ", accessor.GetCorrelationId());
    }

    private sealed class TestBeanUpperCase : IMessageProcessor
    {
        public object ProcessMessage(IMessage message)
        {
            var str = message.Payload as string;
            return str.ToUpper();
        }
    }

    private sealed class TestBeanCreateMessage : IMessageProcessor
    {
        public object ProcessMessage(IMessage message)
        {
            var str = message.Payload as string;
            return IntegrationMessageBuilder.WithPayload(str).SetCorrelationId("456-XYZ").Build();
        }
    }
}
