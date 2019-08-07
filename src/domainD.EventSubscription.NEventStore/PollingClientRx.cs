using NEventStore;
using NEventStore.Persistence;
using NEventStore.PollingClient;
using System;
using System.Reactive.Subjects;
using System.Threading;

namespace domainD.EventSubscription.NEventStore
{
    public sealed class PollingClientRx
    {
        private readonly PollingClient2 _pollingClient2;

        private readonly Subject<ICommit> _subject;

        public PollingClientRx(IPersistStreams persistStreams, int pollingInterval = 1000, CancellationToken cancellationToken = default)
        {
            if (persistStreams == null) throw new ArgumentNullException(nameof(persistStreams));
            if (pollingInterval <= 0)
            {
                throw new ArgumentException("Must be greater than 0", nameof(pollingInterval));
            }

            _subject = new Subject<ICommit>();
            _pollingClient2 = new PollingClient2(persistStreams, c =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _subject.OnCompleted();
                        return PollingClient2.HandlingResult.Stop;
                    }

                    _subject.OnNext(c);
                    return PollingClient2.HandlingResult.MoveToNext;
                },
                waitInterval: pollingInterval);
        }

        public IDisposable Subscribe(IObserver<ICommit> observer)
        {
            return _subject.Subscribe(observer);
        }

        private long _checkpointToObserveFrom;

        public IObservable<ICommit> ObserveFrom(long checkpointToken = 0)
        {
            _checkpointToObserveFrom = checkpointToken;
            return _subject;
        }

        internal void Start()
        {
            _pollingClient2.StartFrom(_checkpointToObserveFrom);
        }

        internal void Dispose()
        {
            _pollingClient2?.Dispose();
            _subject?.Dispose();
        }
    }
}
