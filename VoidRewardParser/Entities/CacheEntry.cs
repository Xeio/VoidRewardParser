using System;

namespace VoidRewardParser.Entities
{
    public class CacheEntry
    {
        public DateTime Timestamp { get; set; }
        public object Value { get; set; }

        public bool IsExpired(TimeSpan timeout)
        {
            return Timestamp + timeout < DateTime.Now;
        }
    }
}
