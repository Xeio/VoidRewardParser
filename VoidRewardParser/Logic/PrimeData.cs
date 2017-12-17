using HtmlAgilityPack;
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

                var doc = new HtmlDocument();
                doc.LoadHtml(lootFileText);

                var relicTable = doc.DocumentNode.Descendants().First(n => n.InnerText == "Relics:").NextSibling;

                var primeRewardNodes = relicTable.SelectNodes("tr[td[2]]");

                primeData.Primes = primeRewardNodes.Select(node =>
                    new PrimeItem()
                    {
                        Name = node.ChildNodes[0].InnerText,
                        Rarity = node.ChildNodes[1].InnerText.Contains("Rare") ? Rarity.Rare : node.ChildNodes[1].InnerText.Contains("Uncommon") ? Rarity.Uncommon : Rarity.Common
                    })
                    .GroupBy(i => i.Name)
                    .Select(g => g.First())
                    .ToList();

                foreach(var prime in primeData.Primes)
                {
                    prime.Name = prime.Name.Replace("Systems Blueprint", "Systems");
                    prime.Name = prime.Name.Replace("Chassis Blueprint", "Chassis");
                    prime.Name = prime.Name.Replace("Neuroptics Blueprint", "Neuroptics");
                }

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
