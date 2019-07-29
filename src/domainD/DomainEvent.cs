using System;

namespace domainD
{
    public abstract class DomainEvent
    {
        public long Version { get; internal set; }

        public Guid AggregateRootId { get; internal set; }

        public Guid Id { get; private set; } = Guid.NewGuid();

        public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;

        public bool IsInitialEvent()
        {
            return Version == AggregateRoot.UnInitializedVersion + 1;
        }
    }
}
