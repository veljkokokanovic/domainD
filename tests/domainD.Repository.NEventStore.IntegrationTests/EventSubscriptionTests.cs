using domainD.EventSubscription;
using domainD.EventSubscription.NEventStore;
using domainD.UnitTests.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace domainD.Repository.NEventStore.IntegrationTests
{
    public class EventSubscriptionTests : TestBase
    {
        private readonly IEventSubscription _eventSubscription;
        private int _eventCounter;
        private readonly EventWaitHandle _waitHandle;

        public EventSubscriptionTests() : base(false)
        {
            _waitHandle = new AutoResetEvent(false);
            var subscriptionBuilder = new EventSubscriptionBuilder();
            subscriptionBuilder.As<IEventSubscriptionBuilder>()
                .On<TestCreated>()
                .HandleAsync(e =>
                {
                    _eventCounter++;
                    return Task.CompletedTask;
                })
                .On<TestDone>()
                .HandleAsync(e =>
                {
                    _eventCounter++;
                    _waitHandle.Set();
                    return Task.CompletedTask;
                })
                .RetryOnError((e, ex) => false);

            ServiceCollection
                .AddSingleton<IEventSubscription, NEventStoreEventSubscription>()
                .AddSingleton<NEventStoreEventSubscription>()
                .AddSingleton(typeof(IEventSubscriptionBuilder), subscriptionBuilder)
                .AddSingleton(typeof(IHandlerResolver), subscriptionBuilder)
                .AddSingleton(subscriptionBuilder.CheckpointLoader);

            _eventSubscription = Services.GetRequiredService<IEventSubscription>();
            _eventSubscription.StartAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task Can_subscribe_to_events()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            await Repository.SaveAsync(e1);
            _waitHandle.WaitOne();
            _eventCounter.Should().Be(2);
        }

        public override void Dispose()
        {
            _eventSubscription?.StopAsync().GetAwaiter().GetResult();
            base.Dispose();
        }
    }
}
