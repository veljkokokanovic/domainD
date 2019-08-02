using System;

namespace domainD
{
    public abstract class DomainEvent
    {
        public long Version { get; set; } = AggregateRoot.UnInitializedVersion + 1;

        public Guid AggregateRootId { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public bool IsInitialEvent()
        {
            return Version == AggregateRoot.UnInitializedVersion + 1;
        }
    }
}
