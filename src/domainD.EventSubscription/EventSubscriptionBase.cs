using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace domainD.EventSubscription
{
    public abstract class EventSubscriptionBase : IEventSubscription
    {
        protected EventSubscriptionBase()
        {

        }

        internal EventSubscriptionBuilder Builder { get; }

        public abstract Task StartAsync(CancellationToken cancellationToken = default);
        public abstract Task StopAsync(CancellationToken cancellationToken = default);
        async Task IEventSubscription.HandleAsync<TEvent>(TEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}
