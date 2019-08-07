using Fasterflect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace domainD
{
    public abstract class AggregateRoot : Entity<Guid>, IAggregateRoot
    {
        public const int UnInitializedVersion = -1;

        private ReplaySubject<DomainEvent> _subject = new ReplaySubject<DomainEvent>();

        public static TAggregateRoot Create<TAggregateRoot>(DomainEvent @event, Guid id = default)
            where TAggregateRoot : Entity<Guid>, IAggregateRoot
        {
            if (@event.Version != UnInitializedVersion + 1)
            {
                throw new EventVersionMismatchException("Invalid version of creation event. Expected {Version + 1}, but was {@event.Version}", @event.Version, UnInitializedVersion + 1);
            }

            var aggregateRoot = CreateImpl<TAggregateRoot>(id);
            aggregateRoot.RaiseEvent(@event);
            return aggregateRoot;
        }

        public static TAggregateRoot CreateFromHistory<TAggregateRoot>(DomainEvent[] events)
            where TAggregateRoot : Entity<Guid>, IAggregateRoot
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            if (!events.Any())
            {
                throw new ArgumentException("Event sequence is empty", nameof(events));
            }

            var aggregateRoot = CreateImpl<TAggregateRoot>(events.First().AggregateRootId);

            foreach (var evt in events)
            {
                aggregateRoot.Handle(evt);
            }

            return aggregateRoot;
        }

        private static TAggregateRoot CreateImpl<TAggregateRoot>(Guid id = default)
            where TAggregateRoot : Entity<Guid>, IAggregateRoot
        {
            try
            {
                if (id == default)
                {
                    id = Guid.NewGuid();
                }

                return (TAggregateRoot)Activator.CreateInstance(
                    typeof(TAggregateRoot),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new object[] { id },
                    CultureInfo.CurrentCulture);
            }
            catch (TargetInvocationException tex)
            {
                if (tex.InnerException is ArgumentException)
                {
                    throw tex.InnerException;
                }

                throw;
            }
        }

        protected AggregateRoot(Guid id) : base(id)
        {
            EventDispatcher = this;
            ConnectEventHandlers();
        }

        public long Version { get; private set; } = UnInitializedVersion;

        void IAggregateRoot.Subscribe(Action<DomainEvent> action)
        {
            var replay = _subject.Replay();
            replay.Subscribe(action);
            replay.Connect().Dispose();
        }

        void IAggregateRoot.Subscribe(Func<DomainEvent,Task> action, CancellationToken cancellationToken = default)
        {
             var replay = _subject.Replay();
            replay
                .Select(e => Observable.FromAsync(async () => await action(e)))
                .Concat()
                .ObserveOn(Scheduler.CurrentThread)
                .Subscribe(cancellationToken);
            replay.Connect().Dispose();
        }

        void IAggregateRoot.ClearEvents()
        {
            _subject.Dispose();
            _subject = new ReplaySubject<DomainEvent>();
            ConnectEventHandlers();
        }

        private void ConnectEventHandlers()
        {
            _subject
                .Subscribe(@event =>
                {
                    @event.Version = Version + 1;
                    @event.AggregateRootId = Identity;
                    ((IAggregateRoot)this).Handle(@event);
                });
        }

        void IAggregateRoot.Handle(DomainEvent @event)
        {
            if (@event.Version != Version + 1)
            {
                throw new EventVersionMismatchException($"Invalid event version during handler execution. Expected {Version + 1}, but was {@event.Version}", @event.Version, Version + 1);
            }

            if (@event.AggregateRootId != Identity)
            {
                throw new InvalidOperationException($"Event aggregate root Id mismatch. Expected {Identity}, but was {@event.AggregateRootId}");
            }

            try
            {
                this.CallMethod("Handle", @event);
            }
            catch (TargetInvocationException tex) when (tex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(tex.InnerException).Throw();
                throw;
            }

           
            Version = @event.Version;
            SetEventDispatcher(this);
        }

        void IEventDispatcher.DispatchEvent(DomainEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            _subject.OnNext(@event);
        }

        private void SetEventDispatcher(IEventDispatcher dispatcher)
        {
            foreach (var entity in GetEntities(this))
            {
                entity.EventDispatcher = dispatcher;
            }
        }

        private static IEnumerable<Entity> GetEntities(object o)
        {
            foreach (var member in o.GetType().FieldsAndProperties(Flags.InstanceAnyVisibility))
            {
                if (typeof(IEnumerable).IsAssignableFrom(member.Type()))
                {
                    if (o.TryGetValue(member.Name, Flags.InstanceAnyVisibility) is IEnumerable enumerable)
                    {
                        foreach (var instance in enumerable)
                        {
                            if (instance != null)
                            {
                                foreach (var obj in GetEntities(instance))
                                {
                                    yield return obj;
                                }
                            }
                        }
                    }
                }
                else if (typeof(Entity).IsAssignableFrom(member.Type()))
                {
                    yield return o.TryGetValue(member.Name, Flags.InstanceAnyVisibility) as Entity;
                }
            }
        }
    }
}
