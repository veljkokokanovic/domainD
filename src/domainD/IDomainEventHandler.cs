using System.Threading.Tasks;

namespace domainD
{
    public interface IDomainEventHandler<T>
        where T : DomainEvent
    {
        Task HandleAsync(T domainEvent);
    }
}
