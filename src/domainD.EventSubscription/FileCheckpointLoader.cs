using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace domainD.EventSubscription
{
    internal class FileCheckpointLoader : ICheckpointLoader
    {
        private readonly string _fileName;

        public FileCheckpointLoader(string filename = null)
        {
            _fileName = filename ?? "checkpoint";

            if (string.IsNullOrWhiteSpace(_fileName))
            {
                throw new ArgumentException(nameof(filename));
            }
        }

        public Task<T> LoadAsync<T>()
        {
            if (File.Exists(_fileName))
            {
                var checkpointToken = File.ReadLines(_fileName).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(checkpointToken))
                {
                    return Task.FromResult(Parse<T>(checkpointToken));
                }
            }

            return Task.FromResult(default(T));
        }

        public Task SaveAsync<T>(T checkpointToken)
        {
            if (checkpointToken.Equals(default(T)))
            {
                throw new ArgumentNullException(nameof(checkpointToken));
            }

            File.WriteAllText(_fileName, checkpointToken.ToString());
            return Task.CompletedTask;
        }

        public static T Parse<T>(string value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
        }
    }
}
