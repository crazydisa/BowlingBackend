using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Bowling: DictionaryItem
    {

        public ICollection<Participation> Participations { get; set; } = new List<Participation>();
    }
}
