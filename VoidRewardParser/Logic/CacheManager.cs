using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using VoidRewardParser.Entities;

namespace VoidRewardParser.Logic
{
    public class FileCacheManager
    {
        private SemaphoreSlim _cacheLock = new SemaphoreSlim(1);

        private TimeSpan _expirationTimespan = TimeSpan.Parse(ConfigurationManager.AppSettings["CacheExpiration"]);

        public static FileCacheManager Instance { get; set; } = new FileCacheManager();

        Dictionary<string, CacheEntry> _values = new Dictionary<string, CacheEntry>();

        public async Task<T> GetValue<T>(string cacheKey, Func<Task<T>> delegatePopulator)
        {
            CacheEntry cacheEntry;
            if (_values.TryGetValue(cacheKey, out cacheEntry))
            {
                if (!cacheEntry.IsExpired(_expirationTimespan))
                {
                    return (T)cacheEntry.Value;
                }
            }

            try
            {
                await _cacheLock.WaitAsync();

                if (_values.TryGetValue(cacheKey, out cacheEntry))
                {
                    if (!cacheEntry.IsExpired(_expirationTimespan))
                    {
                        return (T)cacheEntry.Value;
                    }
                }

                string filename = Path.ChangeExtension(cacheKey, ".data");

                cacheEntry = await Task.Run(() => TryGetFromFile<T>(filename));

                if (cacheEntry == null)
                {
                    cacheEntry = new CacheEntry() { Timestamp = DateTime.Now, Value = await delegatePopulator() };
                    WriteToFile(filename, cacheEntry.Value);
                }

                if (cacheEntry != null)
                {
                    _values[cacheKey] = cacheEntry;
                    return (T)cacheEntry.Value;
                }
            }
            finally
            {
                _cacheLock.Release();
            }

            return default(T);
        }

        public CacheEntry TryGetFromFile<T>(string filename)
        {
            if (File.Exists(filename))
            {
                FileInfo fileInfo = new FileInfo(filename);
                if (fileInfo.LastWriteTime + _expirationTimespan > DateTime.Now)
                {
                    //Not expired
                    var formatter = new BinaryFormatter();
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        object value = formatter.Deserialize(stream);
                        return new CacheEntry() { Timestamp = fileInfo.LastWriteTime, Value = value };
                    }
                }
            }
            return null;
        }

        public void WriteToFile<T>(string filename, T value)
        {
            //Not expired
            var formatter = new BinaryFormatter();
            using (var stream = File.OpenWrite(filename))
            {
                formatter.Serialize(stream, value);
            }
        }
    }
}
