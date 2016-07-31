using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VoidRewardParser.Entities;

namespace VoidRewardParser.Logic
{
    [Serializable]
    public class PrimeData
    {
        public List<PrimeItem> Primes { get; set; }

        public static async Task<PrimeData> Load()
        {
            string lootFileText = await DownloadLootFile();
            
            var lootFileLines = lootFileText.Split(new []{ '\n'}, StringSplitOptions.RemoveEmptyEntries);

            var primeLootLines = lootFileLines.Where(line => line.Contains("PRIME"));

            var primeItems = primeLootLines.Select(l => TryParseLine(l)).Where(i => i != null);

            var uniquePrimes = primeItems.GroupBy(i => i.Name).Select(group => group.First());

            return new PrimeData() { Primes = uniquePrimes.ToList() };
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
    }
}
