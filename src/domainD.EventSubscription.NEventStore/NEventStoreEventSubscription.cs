using Fasterflect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NEventStore;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace domainD.EventSubscription.NEventStore
{
    public class NEventStoreEventSubscription : IEventSubscription
    {
        private readonly IServiceProvider _serviceProvider;
        private IStoreEvents _eventStore;
        private PollingClientRx _pollingClient;
        private IDisposable _pollingSubscription;
        private readonly ILogger _logger;

        public NEventStoreEventSubscription(IServiceProvider serviceProvider, ILogger logger = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger ?? NullLogger.Instance;
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _eventStore = _serviceProvider.GetRequiredService<IStoreEvents>();
            _pollingClient = new PollingClientRx(_eventStore.Advanced, cancellationToken: cancellationToken);
            _pollingSubscription = _pollingClient.ObserveFrom().SubscribeAsync(CommitHandler, ErrorHandler, WhenDone);
            _pollingClient.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _pollingSubscription?.Dispose();
            _pollingClient?.Dispose();
            _eventStore?.Advanced.Dispose();
            return Task.CompletedTask;
        }

        private async Task CommitHandler(ICommit commit)
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

                        var handlerTask = (Task)handler.DynamicInvoke(new[]{evt}.Concat(resolvableParameters).ToArray());
                        await handlerTask.ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogWarning("Subscription event handler for {EventType} was not registered", evt.GetType().Name);
                    }
                }

            }
        }

        private void ErrorHandler(Exception ex)
        {
            _logger.LogCritical(ex, "Error during handling event subscription");
            var builder = _serviceProvider.GetRequiredService<IHandlerResolver>();
            if (builder.TryGetErrorHandler(out var handler))
            {
                handler.DynamicInvoke(ex);
            }
            else
            {
                _logger.LogWarning("Error handler for was not registered");
                ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }

        private void WhenDone()
        {

        }
    }
}
