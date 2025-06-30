using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Team: Object
    {
        [displayExpr]
        public string? Name { get; set; }
        public string? SportType { get; set; }

        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<Participation> TeamParticipations { get; set; } = new List<Participation>();
        public ICollection<EventTeamMember> EventTeamMembers { get; set; } = new List<EventTeamMember>();

    }
}
