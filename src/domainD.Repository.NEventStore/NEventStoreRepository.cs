using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NEventStore;

namespace domainD.Repository.NEventStore
{
    public class NEventStoreRepository<TAggregateRoot> : RepositoryBase<TAggregateRoot>
        where TAggregateRoot : Entity<Guid>, IAggregateRoot
    {
        private readonly IStoreEvents _eventStore;
        private readonly ILogger _logger;

        public NEventStoreRepository(IStoreEvents eventStore, IServiceProvider serviceProvider, ILogger logger = null) : base(serviceProvider)
        {
            _eventStore = eventStore;
            _logger = logger ?? NullLogger.Instance;
        }

        public override Task SaveAsync(TAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException(nameof(aggregateRoot));
            }

            var uncommittedEvents = DispatchEvents(aggregateRoot).ToArray();

            if (uncommittedEvents.Any())
            {
                using (var stream = _eventStore.OpenStream(typeof(TAggregateRoot).Name, aggregateRoot.Identity,(int)uncommittedEvents.First().Version))
                {
                    var commitId = OperationContext.CommandId ?? Guid.NewGuid();

                    foreach (var @event in uncommittedEvents)
                    {
                        if (OperationContext.UserId.HasValue)
                        {
                            @event.CreatedBy = OperationContext.UserId.Value;
                        }

                        stream.Add(new EventMessage
                        {
                            Body = @event,
                            Headers = new Dictionary<string, object>
                            {
                                { KnownHeaders.EventClrType, @event.GetType().AssemblyQualifiedName },
                                { KnownHeaders.AggregateRootClrType, typeof(TAggregateRoot).AssemblyQualifiedName },
                                { KnownHeaders.CorrelationId, commitId }
                            }
                        });
                    }

                    try
                    {
                        stream.CommitChanges(commitId);
                    }
                    catch (DuplicateCommitException dce)
                    {
                        stream.ClearChanges();
                        _logger.LogWarning($"Duplicate commit {commitId} detected, skipping...", dce);
                    }
                    catch (global::NEventStore.ConcurrencyException cex)
                    {
                        stream.ClearChanges();
                        throw new ConcurrencyException(cex.Message, cex);
                    }
                    catch
                    {
                        stream.ClearChanges();
                        throw;
                    }
                    finally
                    {
                        aggregateRoot.ClearEvents();
                    }
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

            using (var stream = _eventStore.OpenStream(typeof(TAggregateRoot).Name, aggregateRootId, 0))
            {
                var events = stream.CommittedEvents.Select(ce => ce.Body).Cast<DomainEvent>().ToArray();
                if(!events.Any())
                {
                    return Task.FromResult<TAggregateRoot>(null);
                }
                var aggregateRoot = AggregateRoot.CreateFromHistory<TAggregateRoot>(events);
                return Task.FromResult(aggregateRoot);
            }
        }
    }
}
