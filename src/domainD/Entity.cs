using System;
using System.Reactive.Subjects;
using System.Threading;

namespace domainD
{
    public abstract class Entity
    {
        protected static readonly AsyncLocal<ReplaySubject<DomainEvent>> Subject = new AsyncLocal<ReplaySubject<DomainEvent>>();

        private readonly Guid _aggregateRootId; 

        protected Entity(Guid aggregateRootId)
        {
            if (aggregateRootId == default)
            {
                throw new ArgumentException("Aggregate root Id must not be empty guid.", nameof(aggregateRootId));
            }

            _aggregateRootId = aggregateRootId;
            Subject.Value = Subject.Value ?? new ReplaySubject<DomainEvent>();
        }

        protected internal virtual void RaiseEvent(DomainEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            @event.AggregateRootId = _aggregateRootId;
            Subject.Value.OnNext(@event);
        }
    }
}
