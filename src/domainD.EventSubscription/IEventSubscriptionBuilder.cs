using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domainD.EventSubscription
{
    public interface IEventSubscriptionBuilder
    {
        IEventHandler<TEvent> On<TEvent>() where TEvent : DomainEvent;

        IEventSubscriptionBuilder RetryOnError(Func<DomainEvent, Exception, bool> retryResolver);
    }

    internal interface IHandlerResolver
    {
        bool TryGetEventHandler(DomainEvent @event, out Delegate handler);

        bool TryGetErrorHandler(out Delegate handler);
    }

    public interface IEventHandler<TEvent> where TEvent : DomainEvent
    {
        IEventSubscriptionBuilder HandleAsync(Func<TEvent, Task> handler);

        IEventSubscriptionBuilder HandleAsync<T1>(Func<TEvent, T1, Task> handler);

        IEventSubscriptionBuilder HandleAsync<T1, T2>(Func<TEvent, T1, T2, Task> handler);

        IEventSubscriptionBuilder HandleAsync<T1, T2, T3>(Func<TEvent, T1, T2, T3, Task> handler);

        IEventSubscriptionBuilder HandleAsync<T1, T2, T3, T4>(Func<TEvent, T1, T2, T3, T4, Task> handler);
    }
}
