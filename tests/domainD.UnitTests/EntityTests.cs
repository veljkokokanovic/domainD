using System;
using System.Collections.Generic;
using domainD.UnitTests.Entities;
using Fasterflect;
using FluentAssertions;
using Xunit;

namespace domainD.UnitTests
{
    public class EntityTests
    {
        [Fact]
        public void Can_create_aggregate_root_from_event()
        {
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test one", 1));
            e1.Version.Should().Be(AggregateRoot.UnInitializedVersion + 1);
            e1.IsDone.Should().BeFalse();
            e1.Name.Should().Be("test one");
            e1.Count.Should().Be(1);
            e1.Subscribe(e => e.Version.Should().Be(AggregateRoot.UnInitializedVersion + 1));
        }

        [Fact]
        public void Can_create_aggregate_root_from_event_and_modify_state()
        {
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2));
            e1.Done();
            e1.Property.SetName("property name");
            e1.Version.Should().Be(2);
            e1.IsDone.Should().BeTrue();
            e1.Name.Should().Be("test two");
            e1.Count.Should().Be(2);
            e1.Property.Name.Should().Be("property name");
            var eventVersionCounter = 0;
            e1.Subscribe(e => e.Version.Should().Be(eventVersionCounter++));
            eventVersionCounter.Should().Be(3);
        }

        [Fact]
        public void Can_create_and_modify_multiple_aggregate_roots()
        {
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test three", 3));
            e1.Done();

            var e2 = AggregateRoot.Create<TestEntity>(new TestCreated("test three", 4));
            e2.Property.SetName("name");

            e1.Version.Should().Be(1);
            e2.Version.Should().Be(1);
            e1.Name.Should().Be("test three");
            e2.Name.Should().Be("test three");
            e1.Count.Should().Be(3);
            e2.Count.Should().Be(4);
            e1.IsDone.Should().BeTrue();
            e2.IsDone.Should().BeFalse();
            e1.Property.Name.Should().BeNullOrEmpty();
            e2.Property.Name.Should().Be("name");

            var eventVersionCounter = 0;
            e1.Subscribe(e => e.Version.Should().Be(eventVersionCounter++));
            eventVersionCounter.Should().Be(2);
            eventVersionCounter = 0;
            e2.Subscribe(e => e.Version.Should().Be(eventVersionCounter++));
            eventVersionCounter.Should().Be(2);
        }

        [Fact]
        public void Can_create_aggregate_root_from_history()
        {
            var aggregateRootId = Guid.NewGuid();
            var history = new List<DomainEvent>
            {
                new TestCreated("test three", 3),
                new TestDone(),
                new NameSet("name")
            };
            long versionCounter = 0;
            history.ForEach(e =>
            {
                e.SetPropertyValue("AggregateRootId", aggregateRootId);
                e.SetPropertyValue("Version", versionCounter++);
            });

            var aggregateRoot = AggregateRoot.CreateFromHistory<TestEntity>(history.ToArray());

            aggregateRoot.Version.Should().Be(2);
            aggregateRoot.IsDone.Should().BeTrue();
            aggregateRoot.Name.Should().Be("test three");
            aggregateRoot.Count.Should().Be(3);
            aggregateRoot.Property.Name.Should().Be("name");
            var eventVersionCounter = 0;
            aggregateRoot.Subscribe(e =>eventVersionCounter++);
            // we can subscribe only to events that have been created though actions on aggregate
            // here were rebuilding state based on events that have been already processed
            eventVersionCounter.Should().Be(0);
        }

        [Fact]
        public void Missing_event_handler_throws_MissingMethodException()
        {
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test three", 3));
            Action act = () => e1.NonHandle();
            act.Should().Throw<MissingMethodException>();
        }
    }
}
