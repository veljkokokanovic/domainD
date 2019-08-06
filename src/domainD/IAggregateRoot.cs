using System;
using System.Threading;
using System.Threading.Tasks;

namespace domainD
{
    public interface IAggregateRoot : IEventDispatcher
    {
        void Subscribe(Action<DomainEvent> action);

        void Subscribe(Func<DomainEvent,Task> action, CancellationToken cancellationToken = default);

        void Handle(DomainEvent @event);

        void ClearEvents();

        long Version { get; }
    }
}
