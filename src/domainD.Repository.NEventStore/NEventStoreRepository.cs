using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NEventStore;

namespace domainD.Repository.NEventStore
{
    public class NEventStoreRepository<TAggregateRoot> : RepositoryBase<TAggregateRoot>
        where TAggregateRoot : Entity<Guid>, IAggregateRoot
    {
        private readonly IStoreEvents _eventStore;

        public NEventStoreRepository(IStoreEvents eventStore, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _eventStore = eventStore;
        }

        public override Task SaveAsync(TAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException(nameof(aggregateRoot));
            }

            var uncommittedEvents = DispatchEvents(aggregateRoot);

            if (uncommittedEvents.Any())
            {
                using (var stream = _eventStore.CreateStream(typeof(TAggregateRoot).FullName, aggregateRoot.Identity))
                {
                    foreach (var @event in uncommittedEvents)
                    {
                        stream.Add(new EventMessage
                        {
                            Body = @event,
                            Headers = new Dictionary<string, object>
                            {
                                { KnownHeaders.EventClrType, @event.GetType().AssemblyQualifiedName },
                                { KnownHeaders.AggregateRootClrType, typeof(TAggregateRoot).AssemblyQualifiedName },
                                { KnownHeaders.CorrelationId, OperationContext.CorrelationId ?? Guid.NewGuid() }
                            }
                        });
                    }

                    stream.CommitChanges(OperationContext.CommandId ?? Guid.NewGuid());
                }
            }

            return Task.CompletedTask;
        }

        public override Task<TAggregateRoot> GetAsync(Guid aggregateRootId)
        {
            if (aggregateRootId == default)
            {
                throw new ArgumentNullException(nameof(aggregateRootId));
            }

            using (var stream = _eventStore.OpenStream(typeof(TAggregateRoot).FullName, aggregateRootId, 0))
            {
                var events = stream.CommittedEvents.Select(ce => ce.Body).Cast<DomainEvent>();
                var aggregateRoot = AggregateRoot.CreateFromHistory<TAggregateRoot>(events.ToArray());
                return Task.FromResult(aggregateRoot);
            }
        }
    }
}
