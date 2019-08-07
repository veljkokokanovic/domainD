using domainD.UnitTests.Entities;
using Fasterflect;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace domainD.Repository.NEventStore.IntegrationTests
{
    public class DomainEventHandlerTests : TestBase
    {
        public DomainEventHandlerTests()
        {
            ServiceCollection.AddTransient(typeof(IDomainEventHandler<TestDone>), typeof(TestDoneEventHandler));
        }

        [Fact]
        public async Task Domain_event_handler_creates_new_aggregateroot()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test domain eh", 2), id);
            e1.Done();
            await Repository.SaveAsync(e1);
            var repository = Services.GetRequiredService<IRepository<NewAggregateRoot>>();
            var n = await repository.GetAsync(TestDoneEventHandler.AggregateRootId);
            n.Should().NotBeNull();
            n.GetPropertyValue("FinallyDone").As<bool>().Should().BeTrue();
        }

        [Fact]
        public async Task Domain_event_handler_error_rollbacks_transaction()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test domain eh", 2), id);
            e1.Done(true);
            Action save = () => Repository.SaveAsync(e1).GetAwaiter().GetResult();
            save.Should().Throw<Exception>().WithMessage("error");
            (await Repository.GetAsync(id)).Should().BeNull();
            (await Repository.GetAsync(TestDoneEventHandler.AggregateRootId)).Should().BeNull();
        }
    }
}
