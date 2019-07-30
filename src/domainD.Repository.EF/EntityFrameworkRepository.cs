using System;
using System.Threading.Tasks;

namespace domainD.Repository.EF
{
    public class EntityFrameworkRepository<TAggregateRoot> : IRepository<TAggregateRoot>
        where TAggregateRoot : Entity<Guid>, IAggregateRoot
    {
        public Task<TAggregateRoot> GetAsync(Guid aggregateRootId)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(TAggregateRoot aggregateRoot)
        {
            throw new NotImplementedException();
        }
    }
}
