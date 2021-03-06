﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace domainD.EventSubscription
{
    public static class HostingExtensions
    {
        public static IHostBuilder AddEventSubscription<TSubscription>(
            this IHostBuilder builder, 
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

                if (subscriptionBuilder.CheckpointLoaderType != null)
                {
                    services.AddScoped(typeof(ICheckpointLoader), subscriptionBuilder.CheckpointLoaderType);
                }
                else
                {
                    services.AddSingleton(subscriptionBuilder.CheckpointLoader);
                }
            });
            

            return builder;
        }
    }
}
