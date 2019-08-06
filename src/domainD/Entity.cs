using System;

namespace domainD
{
    public abstract class Entity
    {
        internal IEventDispatcher EventDispatcher { get; set; }

        protected internal virtual void RaiseEvent(DomainEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (EventDispatcher == null)
            {
                throw new InvalidOperationException($"Event dispatcher on {GetType()} is not set.");
            }

            EventDispatcher.DispatchEvent(@event);
        }
    }
}
