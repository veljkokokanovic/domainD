using System;
using System.Collections.Generic;
using System.Text;

namespace domainD.UnitTests.Entities
{
    public class NewAggregateRoot : AggregateRoot
    {
        protected NewAggregateRoot(Guid id) : base(id)
        {
        }

        internal bool FinallyDone { get; set; }

        void Handle(TestEntityDoneCompleted @event)
        {
            FinallyDone = true;
        }
    }

    public class TestEntityDoneCompleted : DomainEvent
    {
        
    }
}
