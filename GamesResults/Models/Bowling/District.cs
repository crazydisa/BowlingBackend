using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class District : DictionaryItem
    {

        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
