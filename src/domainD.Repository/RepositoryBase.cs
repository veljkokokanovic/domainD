using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace domainD.Repository
{
    public abstract class RepositoryBase<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot
        : Entity<Guid>, IAggregateRoot
    {
        private readonly IServiceProvider _serviceProvider;

        protected RepositoryBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public abstract Task SaveAsync(TAggregateRoot aggregateRoot);
        public abstract Task<TAggregateRoot> GetAsync(Guid aggregateRootId);

        protected virtual IEnumerable<DomainEvent> DispatchEvents(TAggregateRoot aggregateRoot)
        {
            var uncommittedEvents = new List<DomainEvent>();
            aggregateRoot.Subscribe(async e =>
            {
                foreach (var handler in GetEventHandlers(e))
                {
                    try
                    {
                        await GetHandlerRunner(handler, e).ConfigureAwait(false);
                    }
                    catch (TargetInvocationException tex) when (tex.InnerException != null)
                    {
                        ExceptionDispatchInfo.Capture(tex.InnerException).Throw();
                    }
                }
                uncommittedEvents.Add(e);
            });

            return uncommittedEvents;
        }

        private IEnumerable GetEventHandlers(DomainEvent @event)
        {
            var domainEventHandlerType = typeof(IDomainEventHandler<>).MakeGenericType(@event.GetType());
            var allEventHandlers = typeof(IEnumerable<>).MakeGenericType(domainEventHandlerType);
            return (IEnumerable)_serviceProvider.GetService(allEventHandlers);
        }

        private Task GetHandlerRunner(object handler, DomainEvent @event)
        {
            return (Task)handler.GetType().InvokeMember(
                "HandleAsync",
                BindingFlags.InvokeMethod,
                null,
                handler,
                new object[] { @event });
        }
    }
}
