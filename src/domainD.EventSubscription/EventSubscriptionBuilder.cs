using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace domainD.EventSubscription
{
    internal class EventSubscriptionBuilder : IEventSubscriptionBuilder, IHandlerResolver
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        IEventHandler<TEvent> IEventSubscriptionBuilder.On<TEvent>()
        {
            return new EventHandler<TEvent>(this);
        }

        IEventSubscriptionBuilder IEventSubscriptionBuilder.RetryOnError(Func<DomainEvent, Exception, bool> retryResolver)
        {
            _handlers[typeof(Exception)] = retryResolver;
            return this;
        }

        bool IHandlerResolver.TryGetEventHandler(DomainEvent @event, out Delegate handler)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            return _handlers.TryGetValue(@event.GetType(), out handler);
        }

        bool IHandlerResolver.TryGetErrorHandler(out Delegate handler)
        {
            return _handlers.TryGetValue(typeof(Exception), out handler);
        }

        private class EventHandler<TEvent> : IEventHandler<TEvent> 
            where TEvent : DomainEvent
        {
            private readonly EventSubscriptionBuilder _builder;

            public EventHandler(EventSubscriptionBuilder builder)
            {
                _builder = builder;
            }

            public IEventSubscriptionBuilder HandleAsync(Func<TEvent, Task> handler)
            {
                _builder._handlers[typeof(TEvent)] = handler;
                return _builder;
            }

            IEventSubscriptionBuilder IEventHandler<TEvent>.HandleAsync<T1>(Func<TEvent, T1, Task> handler)
            {
                _builder._handlers[typeof(TEvent)] = handler;
                return _builder;
            }

            IEventSubscriptionBuilder IEventHandler<TEvent>.HandleAsync<T1, T2>(Func<TEvent, T1, T2, Task> handler)
            {
                _builder._handlers[typeof(TEvent)] = handler;
                return _builder;
            }

            IEventSubscriptionBuilder IEventHandler<TEvent>.HandleAsync<T1, T2, T3>(Func<TEvent, T1, T2, T3, Task> handler)
            {
                _builder._handlers[typeof(TEvent)] = handler;
                return _builder;
            }

            public IEventSubscriptionBuilder HandleAsync<T1, T2, T3, T4>(Func<TEvent, T1, T2, T3, T4, Task> handler)
            {
                _builder._handlers[typeof(TEvent)] = handler;
                return _builder;
            }
        }
    }


}
