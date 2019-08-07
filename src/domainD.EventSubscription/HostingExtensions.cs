using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace domainD.EventSubscription
{
    public static class HostingExtensions
    {
        public static HostBuilder AddEventSubscription<TSubscription>(this HostBuilder builder,
            Action<IEventSubscriptionBuilder> subscriptionConfigurator = null)
         where TSubscription : class, IEventSubscription
        {
            var subscriptionBuilder = new EventSubscriptionBuilder();
            subscriptionConfigurator?.Invoke(subscriptionBuilder);
            builder.ConfigureServices(services =>
            {
                services
                    .AddSingleton<IEventSubscription, TSubscription>()
                    .AddSingleton<TSubscription>()
                    .AddHostedService<EventSubscriptionHostingService<TSubscription>>()
                    .AddSingleton(typeof(IEventSubscriptionBuilder), subscriptionBuilder)
                    .AddSingleton(typeof(IHandlerResolver), subscriptionBuilder);
            });
            

            return builder;
        }
    }
}
