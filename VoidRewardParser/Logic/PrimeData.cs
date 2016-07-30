using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VoidRewardParser.Entities;

namespace VoidRewardParser.Logic
{
    internal static class PrimeData
    {
        public static async Task<List<PrimeItem>> Load()
        {
            string text = string.Empty;
            using (var client = new WebClient())
            {
                string file = ConfigurationManager.AppSettings["PrimeDataFile"];
                var uri = new Uri(file);
                text = await client.DownloadStringTaskAsync(uri);
            }

            var lines = text.Split(new []{ '\n'}, StringSplitOptions.RemoveEmptyEntries);

            var primeLines = lines.Where(line => line.Contains("PRIME"));

            var primes = primeLines.Select(l => TryParseLine(l)).Where(i => i != null);

            var uniquePrimes = primes.GroupBy(i => i.Name).Select(group => group.First());

            return uniquePrimes.ToList();
        }

        private  static PrimeItem TryParseLine(string line)
        {
            //1 MAG PRIME BLUEPRINT, RARE, I: 2 %, E: 4 %, F: 6 %, R: 10 %, 100 Ducats
            var entries = line.Split(',').Select(s => s.Trim()).ToList();
            if (entries.Count >= 7)
            {
                var itemName = entries[0].Substring(2); //trim the number and space from the front
                Rarity rarity = Rarity.Common;
                Enum.TryParse(entries[1], true, out rarity);
                var ducatString = entries[6].Split(' ')[0];
                int ducats;
                int.TryParse(ducatString, out ducats);
                return new PrimeItem() { Name = itemName, Rarity = rarity, Ducats = ducats };
            }
            return null;
        }
    }
}
