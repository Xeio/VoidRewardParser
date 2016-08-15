using System;

namespace VoidRewardParser.Entities
{
    public class CacheEntry<T>
    {
        public DateTime Timestamp { get; set; }
        public T Value { get; set; }

        public CacheEntry()
        {
            Timestamp = DateTime.Now;
        }

        public CacheEntry(T value) : this()
        {
            Value = value;
        }

        public bool IsExpired(TimeSpan timeout)
        {
            return Timestamp + timeout < DateTime.Now;
        }
    }
}
