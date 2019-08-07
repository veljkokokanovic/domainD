using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace domainD.EventSubscription.NEventStore
{
    public static class ObservableExtensions
    {
        public static IDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, Task> onNext, Action<Exception> onError, Action onCompleted)
        {
            return source
                .Select(e => Observable.FromAsync(() => onNext(e)))
                .Concat()
                .Subscribe(
                    e => { },
                    onError,
                    onCompleted);
        }
    }
}
