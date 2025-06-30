using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public enum EventType
    {
        Individual,
        Team
    }

    public class Event: Object
    {
        [displayExpr]
        public string? Name { get; set; }
        public EventType EventType { get; set; }
        public DateTime EventDate { get; set; }
        [ObjectPropertyAttribute(TypeName = "Oil", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long? OilId { get; set; }
        public Oil? Oil { get; set; }

        public ICollection<Participation> Participations { get; set; } = new List<Participation>();
        public ICollection<EventTeamMember> EventTeamMembers { get; set; } = new List<EventTeamMember>();

    }
}
