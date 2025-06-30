using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Oil: DictionaryItem
    {

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
