using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Player: DictionaryItem
    {
        public DateTime? BirthDate { get; set; }
        public string? Country { get; set; }
        [ObjectPropertyAttribute(TypeName = "City", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? CityId { get; set; }
        [ObjectPropertyAttribute(TypeName = "Rank", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? RankId { get; set; }
        public City? City { get; set; }
        public Rank? Rank { get; set; }

        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<Participation> IndividualParticipations { get; set; } = new List<Participation>();
        public ICollection<EventTeamMember> EventTeamMembers { get; set; } = new List<EventTeamMember>();

    }
}
