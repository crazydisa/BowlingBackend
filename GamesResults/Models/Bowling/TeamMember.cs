using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public Team? Team { get; set; }
        [ObjectPropertyAttribute(TypeName = "Player", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        public long PlayerId { get; set; }
        public Player? Player { get; set; }

        // Роль в команде
        public TeamMemberRole Role { get; set; } = TeamMemberRole.Member;
        public bool IsCaptain { get; set; }
        public int OrderNumber { get; set; } // Порядковый номер в составе

        // Статистика в этой команде
        public decimal AverageInTeam { get; set; }
        public int GamesPlayedInTeam { get; set; }
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        // Вычисляемые свойства
        [NotMapped]
        public bool IsActive => JoinedDate <= DateTime.UtcNow;
    }
}
