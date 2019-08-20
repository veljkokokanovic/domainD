using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domainD.Repository.NEventStore
{
    public static class KnownHeaders
    {
        public const string EventClrType = nameof(EventClrType);
        public const string AggregateRootClrType = nameof(AggregateRootClrType);
        public const string CorrelationId = nameof(CorrelationId);
        public const string UserId = nameof(UserId);
    }
}
