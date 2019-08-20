using System;
using System.Collections.Concurrent;
using System.Threading;

namespace domainD
{
    public static class OperationContext
    {
        public static class Keys
        {
            public const string CorrelationIdKey = nameof(CorrelationId);

            public const string CommandIdKey = nameof(CommandId);

            public const string UserIdKey = nameof(UserId);
        }

        private static readonly AsyncLocal<ConcurrentDictionary<string, object>> ContextMap = new AsyncLocal<ConcurrentDictionary<string, object>>();

        public static bool TryGetValue<T>(string key, out T value)
        {
            value = default;

            ContextMap.Value = ContextMap.Value ?? new ConcurrentDictionary<string, object>();

            if (ContextMap.Value.TryGetValue(key, out var rawValue))
            {
                value = (T) rawValue;
                return true;
            }

            return false;
        }

        public static bool TryAddValue<T>(string key, T value)
        {
            ContextMap.Value = ContextMap.Value ?? new ConcurrentDictionary<string, object>();
            return ContextMap.Value.TryAdd(key, value);
        }


        public static Guid? CorrelationId
        {
            get => TryGetValue<Guid?>(Keys.CorrelationIdKey, out var correlationId) ? correlationId : default;
            set => TryAddValue(Keys.CorrelationIdKey, value);
        }

        public static Guid? CommandId
        {
            get => TryGetValue<Guid?>(Keys.CommandIdKey, out var commandId) ? commandId : default;
            set => TryAddValue(Keys.CommandIdKey, value);
        }

        public static Guid? UserId
        {
            get => TryGetValue<Guid?>(Keys.UserIdKey, out var commandId) ? commandId : default;
            set => TryAddValue(Keys.UserIdKey, value);
        }
    }
}
