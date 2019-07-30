using System;

namespace domainD
{
    public interface IAggregateRoot
    {
        void Subscribe(Action<DomainEvent> action);

        void Handle(DomainEvent @event);

        long Version { get; }
    }
}
