using Microsoft.Extensions.DependencyInjection;
using NEventStore;
using NEventStore.Logging;
using NEventStore.Serialization.Json;
using System;

namespace domainD.Repository.NEventStore
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNEventStore(this IServiceCollection services, Action<Wireup> options = null)
        {
            var wireup = Wireup
                .Init()
                .LogToConsoleWindow(LogLevel.Debug)
                .LogToOutputWindow(LogLevel.Debug)
                .UseOptimisticPipelineHook();

            wireup
                .UsingInMemoryPersistence()
                .UsingJsonSerialization();

            options?.Invoke(wireup);
            var es = wireup.Build();
            es.Advanced.Initialize();

            services.AddSingleton(es);

            services.AddTransient(typeof(IRepository<>), typeof(NEventStoreRepository<>));
            return services;
        }
    }
}
