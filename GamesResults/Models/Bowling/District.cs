using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class District : DictionaryItem
    {

        public ICollection<City> Cities { get; set; } = new List<City>();
        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
