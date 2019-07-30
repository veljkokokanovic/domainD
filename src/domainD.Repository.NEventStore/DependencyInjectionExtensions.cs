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

            wireup.UsingInMemoryPersistence().InitializeStorageEngine().UsingJsonSerialization();
            options?.DynamicInvoke(wireup);

            services.AddSingleton(wireup.Build());

            services.AddTransient(typeof(IRepository<>), typeof(NEventStoreRepository<>));
            return services;
        }
    }
}
