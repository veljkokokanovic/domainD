using Fasterflect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NEventStore;
using NEventStore.PollingClient;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace domainD.EventSubscription.NEventStore
{
    public class NEventStoreEventSubscription : IEventSubscription
    {
        private readonly IServiceProvider _serviceProvider;
        private IStoreEvents _eventStore;
        private PollingClient2 _pollingClient;
        private readonly ILogger _logger;

        public NEventStoreEventSubscription(IServiceProvider serviceProvider, ILogger logger = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger ?? NullLogger.Instance;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _eventStore = _serviceProvider.GetRequiredService<IStoreEvents>();
            _pollingClient = new PollingClient2(_eventStore.Advanced,
                c => cancellationToken.IsCancellationRequested ? PollingClient2.HandlingResult.Stop : CommitHandler(c));
            _pollingClient.StartFrom();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _pollingClient?.Stop();
            _pollingClient?.Dispose();
            _eventStore?.Advanced.Dispose();
            return Task.CompletedTask;
        }

        private PollingClient2.HandlingResult CommitHandler(ICommit commit)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var events = commit.Events.Select(e => e.Body).Cast<DomainEvent>();
                var builder = _serviceProvider.GetRequiredService<IHandlerResolver>();
                foreach (var evt in events)
                {
                    if (builder.TryGetEventHandler(evt, out var handler))
                    {
                        var resolvableParameters = handler.Method
                            .Parameters()
                            .Skip(1)
                            .Select(pi => pi.ParameterType)
                            .Select(t => scope.ServiceProvider.GetRequiredService(t))
                            .ToArray();

                        try
                        {
                            var handlerTask =
                                (Task) handler.DynamicInvoke(new[] {evt}.Concat(resolvableParameters).ToArray());
                            handlerTask.GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            return ErrorHandler(evt, ex);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Subscription event handler for {EventType} was not registered",
                            evt.GetType().Name);
                    }
                }

                return PollingClient2.HandlingResult.MoveToNext;
            }
        }

        private PollingClient2.HandlingResult ErrorHandler(DomainEvent evt, Exception ex)
        {
            _logger.LogCritical(ex, "Error during handling {EventType}.", evt.GetType());
            var builder = _serviceProvider.GetRequiredService<IHandlerResolver>();
            if (builder.TryGetErrorHandler(out var handler))
            {
                var result = (bool)handler.DynamicInvoke(evt, ex);
                return result ? PollingClient2.HandlingResult.Retry : PollingClient2.HandlingResult.Stop;
            }

            _logger.LogWarning("Error handler resolver was not registered");
            return PollingClient2.HandlingResult.Stop;
        }
    }
}
