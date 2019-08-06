using System;

namespace domainD.UnitTests.Entities
{
    public class TestEntity : AggregateRoot
    {
        protected TestEntity(Guid id) : base(id)
        {
            Property = new ComplexProperty(id.ToString());
        }

        public void Done(bool raiseError = false)
        {
            RaiseEvent(new TestDone(raiseError));
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
        public TestDone(bool raiseError = false)
        {
            RaiseError = raiseError;
        }

        public bool RaiseError { get; private set; }
    }

    public class NonHandled : DomainEvent
    {

    }
}
