using System.Threading.Tasks;

namespace domainD.EventSubscription
{
    internal class NullCheckpointLoader : ICheckpointLoader
    {
        public Task<T> LoadAsync<T>()
        {
            return Task.FromResult(default(T));
        }

        public Task SaveAsync<T>(T checkpointToken)
        {
            return Task.CompletedTask;
        }
    }
}
