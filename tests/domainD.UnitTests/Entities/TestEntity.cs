using System;

namespace domainD.UnitTests.Entities
{
    public class TestEntity : AggregateRoot
    {
        protected TestEntity(Guid id) : base(id)
        {
            Property = new ComplexProperty(id.ToString(), id);
        }

        public void Done()
        {
            RaiseEvent(new TestDone());
        }

        public void NonHandle()
        {
            RaiseEvent(new NonHandled());
        }

        public bool IsDone { get; private set; }

        public ComplexProperty Property { get; private set; }

        public string Name { get; private set; }

        public int Count { get; private set; }


        protected void Handle(TestCreated @event)
        {
            Name = @event.Name;
            Count = @event.Count;
        }

        protected void Handle(TestDone @event)
        {
            IsDone = true;
        }

        protected void Handle(NameSet @event)
        {
            Property.Name = @event.Name;
        }
    }

    public class TestCreated : DomainEvent
    {
        public TestCreated(string name, int count)
        {
            Name = name;
            Count = count;
        }

        public string Name { get; private set; }

        public int Count { get; private set; }
    }

    public class TestDone : DomainEvent
    {

    }

    public class NonHandled : DomainEvent
    {

    }
}
