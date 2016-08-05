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
        [NonSerialized]
        private string[] _tokens;
        public string[] Tokens
        {
            get
            {
                return _tokens ?? (_tokens = Name.Split(' '));
            }
        }
    }
}