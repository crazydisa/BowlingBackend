using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Participation: Object
    {
        [displayExpr]
        public string? Name => Event != null ? Event.Name + " " + Player != null ? Player.Name : null : null;
        [ObjectPropertyAttribute(TypeName = "Event", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long EventId { get; set; }
        [ObjectPropertyAttribute(TypeName = "Player", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? PlayerId { get; set; }
        [ObjectPropertyAttribute(TypeName = "Team", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? TeamId { get; set; }
        [ObjectPropertyAttribute(TypeName = "Bowling", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long BowlingId { get; set; }
        public string? Result { get; set; }
        public int? Game1 { get; set; }
        public int? Game2 { get; set; }
        public int? Game3 { get; set; }
        public int? Game4 { get; set; }
        public int? Game5 { get; set; }
        public int? Game6 { get; set; }
        public int? Summ { get; set; }
        public double? Average { get; set; }


        public Event? Event { get; set; }
        public Player? Player { get; set; }
        public Team? Team { get; set; }
        public Bowling? Bowling { get; set; }

    }
}
