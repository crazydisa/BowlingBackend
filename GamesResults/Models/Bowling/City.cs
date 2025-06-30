using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class City : DictionaryItem
    {
        [ObjectPropertyAttribute(TypeName = "Distric", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? DistrictId { get; set; }
        public District? District { get; set; }
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}
