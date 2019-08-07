using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace domainD.EventSubscription
{
    internal class EventSubscriptionHostingService<TSubscription> : IHostedService
        where TSubscription : class, IEventSubscription
    {
        private readonly IEventSubscription _subscription;

        public EventSubscriptionHostingService(TSubscription subscription)
        {
            _subscription = subscription;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _subscription.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _subscription.StopAsync(cancellationToken);
        }
    }
}
