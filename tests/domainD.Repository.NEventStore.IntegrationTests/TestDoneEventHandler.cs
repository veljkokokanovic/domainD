using System;
using domainD.UnitTests.Entities;
using System.Threading.Tasks;

namespace domainD.Repository.NEventStore.IntegrationTests
{
    public class TestDoneEventHandler : IDomainEventHandler<TestDone>
    {
        private readonly IRepository<NewAggregateRoot> _repository;
        public static Guid AggregateRootId = Guid.NewGuid();

        public TestDoneEventHandler(IRepository<NewAggregateRoot> repository)
        {
            _repository = repository;
        }

        public Task HandleAsync(TestDone domainEvent)
        {
            if (domainEvent.RaiseError)
            {
                throw new Exception("error");
            }
            var newAggregateRoot = AggregateRoot.Create<NewAggregateRoot>(new TestEntityDoneCompleted(), AggregateRootId);
            return _repository.SaveAsync(newAggregateRoot);
        }
    }
}
