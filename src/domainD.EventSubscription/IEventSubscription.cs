using System;
using System.Threading;
using System.Threading.Tasks;

namespace domainD.EventSubscription
{
    public interface IEventSubscription
    {
        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
