using domainD.UnitTests.Entities;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace domainD.Repository.NEventStore.IntegrationTests
{
    public class RepositoryTests : TestBase
    {
        public RepositoryTests()
        {
        }

        [Fact]
        public async Task Can_save_and_read_aggregate_root()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            await Repository.SaveAsync(e1);
            var ent = await Repository.GetAsync(id);
            e1.Should().BeEquivalentTo(ent);
            ent.Version.Should().Be(1);
        }

        [Fact]
        public async Task Saving_aggregate_root_twice_doesnt_throw()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            await Repository.SaveAsync(e1);
            Action anotherSave = () => Repository.SaveAsync(e1).GetAwaiter().GetResult();
            anotherSave.Should().NotThrow();
            Action get = () => Repository.GetAsync(id).GetAwaiter().GetResult();
            get.Should().NotThrow();
        }

        [Fact]
        public async Task Saving_aggregate_root_twice_within_same_context_doesnt_throw()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            OperationContext.CommandId = Guid.NewGuid();
            await Repository.SaveAsync(e1);
            Action anotherSave = () => Repository.SaveAsync(e1).GetAwaiter().GetResult();
            anotherSave.Should().NotThrow();
            Action get = () => Repository.GetAsync(id).GetAwaiter().GetResult();
            get.Should().NotThrow();
        }

        [Fact]
        public async Task Getting_non_existing_aggregate_returns_null()
        {
            var id = Guid.NewGuid();
            var aggregateRoot = await Repository.GetAsync(id);
            aggregateRoot.Should().BeNull();
        }

        [Fact]
        public async Task Saving_getting_and_modifying_same_aggregate_root()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            await Repository.SaveAsync(e1);
            var e2 = await Repository.GetAsync(id);
            e2.Property.SetName("1");
            await Repository.SaveAsync(e2);
            e2.Property.Name.Should().Be("1");
            e1.Version.Should().Be(1);
            e2.Version.Should().Be(2);
        }

        [Fact]
        public async Task Saving_and_modifying_same_aggregate_root()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            await Repository.SaveAsync(e1);
            e1.Property.SetName("1");
            await Repository.SaveAsync(e1);
            e1.Property.Name.Should().Be("1");
            e1.Version.Should().Be(2);
        }
    }
}
