using System.Threading.Tasks;

namespace domainD.EventSubscription
{
    public interface ICheckpointLoader
    {
        Task<T> LoadAsync<T>();

        Task SaveAsync<T>(T checkpointToken);
    }
}
