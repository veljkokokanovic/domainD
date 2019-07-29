using System;

namespace domainD
{
    public interface IAggregateRoot
    {
        void Subscribe(Action<DomainEvent> action);

        long Version { get; }
    }
}
