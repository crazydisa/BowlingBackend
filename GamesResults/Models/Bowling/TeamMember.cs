using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class TeamMember : Object
    {
        [displayExpr]
        public string? Name => Player != null ? Player.Name : null;
        [ObjectPropertyAttribute(TypeName = "Team", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long TeamId { get; set; }
        [ObjectPropertyAttribute(TypeName = "Player", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long PlayerId { get; set; }
        [ObjectPropertyAttribute(TypeName = "City", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? CityId { get; set; }
        [ObjectPropertyAttribute(TypeName = "Rank", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? RankId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Team? Team { get; set; }
        public Player? Player { get; set; }
        public City? City { get; set; }
        public Rank? Rank { get; set; }
    }
}
