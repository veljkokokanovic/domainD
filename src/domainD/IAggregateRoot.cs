using System;
using System.Threading;
using System.Threading.Tasks;

namespace domainD
{
    public interface IAggregateRoot : IEventDispatcher
    {
        /// <summary>
        /// Replays all uncommited events and applies action supplied
        /// </summary>
        /// <param name="action"></param>
        void Subscribe(Action<DomainEvent> action);

        /// <summary>
        /// Replays all uncommited events and applies async action supplied
        /// </summary>
        /// <param name="action"></param>
        void Subscribe(Func<DomainEvent,Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dispatches event to corrent event handler
        /// </summary>
        /// <param name="event"></param>
        void Handle(DomainEvent @event);

        /// <summary>
        /// Clears all uncommited events
        /// </summary>
        void ClearEvents();

        /// <summary>
        /// Aggregate root version
        /// </summary>
        long Version { get; }
    }
}
