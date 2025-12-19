using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class City : DictionaryItem
    {
        [ObjectPropertyAttribute(TypeName = "Distric", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? DistrictId { get; set; }
        public District? District { get; set; }
        // Боулинг-центры в городе
        public virtual ICollection<Bowling> BowlingCenters { get; set; } = new List<Bowling>();
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
        public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    }
}
