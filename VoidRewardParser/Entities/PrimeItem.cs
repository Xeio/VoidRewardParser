using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidRewardParser.Entities
{
    [Serializable]
    public class PrimeItem
    {
        public string Name { get; set; }
        public Rarity Rarity { get; set; }
        public int Ducats { get; set; }

        public string PlatinumPrice
        {
            get
            {
                if(platPriceData == null)
                {
                    return "...";
                }

                return platPriceData;
            }
            set
            {
                platPriceData = value;
            }
        }

        [NonSerialized]
        private string platPriceData;
    }
}
