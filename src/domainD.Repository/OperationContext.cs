using System;
using System.Threading;

namespace domainD.Repository
{
    public static class OperationContext
    {
        private static readonly AsyncLocal<Guid?> _correlationId = new AsyncLocal<Guid?>();

        private static readonly AsyncLocal<Guid?> _commandId = new AsyncLocal<Guid?>();

        public static Guid? CorrelationId
        {
            get => _correlationId.Value;
            set => _correlationId.Value = value;
        }

        public static Guid? CommandId
        {
            get => _commandId.Value;
            set => _commandId.Value = value;
        }
    }
}
