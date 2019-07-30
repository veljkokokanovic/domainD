using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domainD.Repository.NEventStore
{
    public static class KnownHeaders
    {
        public const string EventClrType = "eventType";
        public const string AggregateRootClrType = "aggregateRootType";
        public const string CorrelationId = "correlationId";
    }
}
