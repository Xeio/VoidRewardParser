using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VoidRewardParser.Entities;

namespace VoidRewardParser.Logic
{
    public static class PlatinumPrices
    {
        private static TimeSpan _expirationTimespan = TimeSpan.Parse(ConfigurationManager.AppSettings["PlatinumCacheExpiration"]);
        private static Dictionary<string, CacheEntry<long?>> _marketCache = new Dictionary<string, CacheEntry<long?>>();
        private const string _baseUrl = "https://warframe.market/api/get_orders/Blueprint/";
        private static readonly string[] _removeBPSuffixPhrases = new[]{
            "Neuroptics", "Chassis", "Systems", "Harness", "Wings"
        };

        private static readonly Dictionary<string, string> _fixedQueryStrings = new Dictionary<string, string>()
        {
            { "Paris Prime Lower Limb", "Paris Prime  Lower Limb" },
            { "Paris Prime Grip", "Paris Prime  Grip" },
            { "Kavasa Prime Band", "Kavasa Prime Collar Band" },
            { "Kavasa Prime Kubrow Collar Blueprint", "Kavasa Prime Collar Blueprint" },
            { "Kavasa Prime Buckle", "Kavasa Prime Collar Buckle" },
            { "Lex Prime Receiver", "Lex Prime Reciever" },
        };

        public static async Task<long?> GetPrimePlatSellOrders(string primeName)
        {
            CacheEntry<long?> cacheItem;
            if (_marketCache.TryGetValue(primeName, out cacheItem))
            {
                if (!cacheItem.IsExpired(_expirationTimespan))
                {
                    return cacheItem.Value;
                }
            }

            var textInfo = new CultureInfo("en-US", false).TextInfo;

            var partName = textInfo.ToTitleCase(primeName.ToLower());

            if (_removeBPSuffixPhrases.Any(suffix => partName.EndsWith(suffix + " Blueprint")))
            {
                partName = partName.Replace(" Blueprint", "");
            }

            // Since Warframe.Market is still using the term Helmet instead of the new one, TODO: this might change
            partName = partName.Replace("Neuroptics", "Helmet");

            if (_fixedQueryStrings.ContainsKey(partName))
            {
                //Some of Warframe.Market's query strings are mangled (extra spaces, misspellings, words missing) fix them manually...
                partName = _fixedQueryStrings[partName];
            }

            string jsonData;
            using (var client = new WebClient())
            {
                var uri = new Uri(_baseUrl + Uri.EscapeDataString(partName));

                try
                {
                    jsonData = await client.DownloadStringTaskAsync(uri);

                    dynamic result = JsonConvert.DeserializeObject(jsonData);

                    // when the server responds anything that is not 200 (HTTP OK) don't bother doing something else
                    if (result.code != 200)
                    {
                        Debug.WriteLine($"Error with {partName}, Status Code: {result.code.Value}");
                        _marketCache[primeName] = new CacheEntry<long?>(null);
                        return null;
                    }

                    IEnumerable<dynamic> sellOrders = result.response.sell;
                    long? smallestPrice = sellOrders.Where(order => order.online_status).Min(order => order.price);

                    _marketCache[primeName] = new CacheEntry<long?>(smallestPrice);
                    return smallestPrice;
                }
                catch
                {
                    return null;
                }
            }            
        }
    }
}