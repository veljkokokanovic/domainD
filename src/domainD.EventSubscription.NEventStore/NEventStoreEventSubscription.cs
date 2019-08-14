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
        private readonly ICheckpointLoader _checkpointLoader;
        private long _currentCheckpoint;

        public NEventStoreEventSubscription(IServiceProvider serviceProvider, ICheckpointLoader checkpointLoader, ILogger logger = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger ?? NullLogger.Instance;
            _checkpointLoader = checkpointLoader;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _currentCheckpoint = await _checkpointLoader.LoadAsync<long>().ConfigureAwait(false);
            _eventStore = _serviceProvider.GetRequiredService<IStoreEvents>();
            _pollingClient = new PollingClient2(
                _eventStore.Advanced,
                c => cancellationToken.IsCancellationRequested ? PollingClient2.HandlingResult.Stop : CommitHandler(c));
            _pollingClient.StartFrom(_currentCheckpoint);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _checkpointLoader.SaveAsync(_currentCheckpoint).ConfigureAwait(false);
            _pollingClient?.Stop();
            _pollingClient?.Dispose();
            _eventStore?.Advanced.Dispose();
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
                                (Task)handler.DynamicInvoke(new[] { evt }.Concat(resolvableParameters).ToArray());
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

                _currentCheckpoint = commit.CheckpointToken;
                return PollingClient2.HandlingResult.MoveToNext;
            }
        }

        private PollingClient2.HandlingResult ErrorHandler(DomainEvent evt, Exception ex)
        {
            var builder = _serviceProvider.GetRequiredService<IHandlerResolver>();
            if (builder.TryGetErrorHandler(out var handler) && (bool)handler.DynamicInvoke(evt, ex))
            {
                _logger.LogError(ex, "Error during handling {EventType}. Will retry.", evt.GetType());
                return PollingClient2.HandlingResult.Retry;
            }

            _logger.LogCritical(ex, "Error during handling {EventType}. Exit polling.", evt.GetType());
            _checkpointLoader.SaveAsync(_currentCheckpoint).GetAwaiter().GetResult();
            return PollingClient2.HandlingResult.Stop;
        }
    }
}
