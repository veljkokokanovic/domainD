using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domainD
{
    public class ObservedDomainEvent
    {
        public DomainEvent DomainEvent { get; set; }

        public bool Handled { get; set; }

        public Guid AggregateRootInstance { get; set; }
    }
}
