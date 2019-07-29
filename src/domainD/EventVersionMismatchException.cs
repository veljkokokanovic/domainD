using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domainD
{
    public class EventVersionMismatchException : Exception
    {
        public EventVersionMismatchException()
        {
        }

        public EventVersionMismatchException(string message)
            : base(message)
        {
        }

        public EventVersionMismatchException(string message, Exception ex)
            : base(message, ex)
        {
        }

        public EventVersionMismatchException(string message, long version, long expectedVersion)
            : base(message)
        {
            Version = version;
            ExpectedVersion = expectedVersion;
        }

        public EventVersionMismatchException(string message, long version, long expectedVersion, Exception innerException)
            : base(message, innerException)
        {
            Version = version;
            ExpectedVersion = expectedVersion;
        }

        public long Version { get; }

        public long ExpectedVersion { get; }
    }
}
