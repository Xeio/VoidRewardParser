using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using VoidRewardParser.Entities;

namespace VoidRewardParser.Logic
{
    [Serializable]
    public class PrimeData
    {
        const string FILE = "PrimeData.data";

        private static TimeSpan _expirationTimespan = TimeSpan.Parse(ConfigurationManager.AppSettings["CacheExpiration"]);
        private static SemaphoreSlim _cacheLock = new SemaphoreSlim(1);

        private static PrimeData _instance;
        public static async Task<PrimeData> GetInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            try
            {
                await _cacheLock.WaitAsync();

                if (_instance == null)
                {
                    _instance = await Load();
                }

                return _instance;

            }
            finally
            {
                _cacheLock.Release();
            }
        }


        public DateTime PrimesLastRetrieved { get; set; }
        public List<PrimeItem> Primes { get; set; }
        public Dictionary<string, ItemSaveData> SavedData { get; set; } = new Dictionary<string, ItemSaveData>();

        public ItemSaveData GetDataForItem(PrimeItem item)
        {
            ItemSaveData data;
            if(!SavedData.TryGetValue(item.Name, out data))
            {
                data = new ItemSaveData();
                SavedData[item.Name] = data;
            }
            return data;
        }

        public static async Task<PrimeData> Load()
        {
            PrimeData primeData = new PrimeData();

            if (File.Exists(FILE))
            {
                var formatter = new BinaryFormatter();
                using (FileStream stream = File.OpenRead(FILE))
                {
                    primeData = formatter.Deserialize(stream) as PrimeData;
                }
            }

            if (primeData.PrimesLastRetrieved + _expirationTimespan < DateTime.Now)
            {
                //Update the cached Primes
                string lootFileText = await DownloadLootFile();

                var lootFileLines = lootFileText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var primeLootLines = lootFileLines.Where(line => line.Contains("PRIME"));

                var primeItems = primeLootLines.Select(l => TryParseLine(l)).Where(i => i != null);

                var uniquePrimes = primeItems.GroupBy(i => i.Name).Select(group => group.First());

                primeData.Primes = uniquePrimes.OrderBy(p => p.Name).ToList();

                primeData.PrimesLastRetrieved = DateTime.Now;
            }

            primeData.SaveToFile();

            return primeData;
        }

        private static async Task<string> DownloadLootFile()
        {
            using (var client = new WebClient())
            {
                string file = ConfigurationManager.AppSettings["PrimeDataFile"];
                var uri = new Uri(file);
                return await client.DownloadStringTaskAsync(uri);
            }
        }

        private static PrimeItem TryParseLine(string line)
        {
            //1 MAG PRIME BLUEPRINT, RARE, I: 2 %, E: 4 %, F: 6 %, R: 10 %, 100 Ducats
            var entries = line.Split(',').Select(s => s.Trim()).ToList();
            if (entries.Count >= 7)
            {
                string itemName;
                if (entries[0].StartsWith("1 "))
                {
                    itemName = entries[0].Substring(2); //trim the number and space from the front
                }
                else
                {
                    itemName = entries[0];
                }
                Rarity rarity = Rarity.Common;
                Enum.TryParse(entries[1], true, out rarity);

                int ducats = 0;
                var ducatSplit = entries[6].Split(' ');
                if (ducatSplit.Length == 2)
                {
                    var ducatString = ducatSplit[0];
                    int.TryParse(ducatString, out ducats);
                }
                
                if (!string.IsNullOrWhiteSpace(itemName))
                {
                    return new PrimeItem() { Name = itemName, Rarity = rarity, Ducats = ducats };
                }
            }
            return null;
        }

        public void SaveToFile()
        {
            var formatter = new BinaryFormatter();
            using (var stream = File.OpenWrite(FILE))
            {
                formatter.Serialize(stream, this);
            }
        }
    }
}
