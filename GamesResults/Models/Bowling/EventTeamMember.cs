using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class EventTeamMember: Object
    {
        [displayExpr]
        public string? Name  => Player != null ? Player.Name : null;
        [ObjectPropertyAttribute(TypeName = "Event", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long EventId { get; set; }
        [ObjectPropertyAttribute(TypeName = "Team", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long TeamId { get; set; }
        [ObjectPropertyAttribute(TypeName = "Player", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long PlayerId { get; set; }

        public Event? Event { get; set; }
        public Team? Team { get; set; }
        public Player? Player { get; set; }

    }
}
