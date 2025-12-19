using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Rank : DictionaryItem
    {

        public ICollection<TeamMember>? TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
