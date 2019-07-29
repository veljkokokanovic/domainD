using System;
using System.Threading.Tasks;

namespace domainD
{
    public interface IRepository<TAggregateRoot>
        where TAggregateRoot : Entity<Guid>, IAggregateRoot
    {
        Task SaveAsync(TAggregateRoot aggregateRoot);

        Task<TAggregateRoot> GetAsync(Guid aggregateRootId);
    }
}
