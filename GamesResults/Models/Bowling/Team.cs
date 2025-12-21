using GamesResults.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace GamesResults.Models.Bowling
{
    public class Team: Object
    {
        [displayExpr]
        public string Name { get; set; } = string.Empty;
        public string? Abbreviation { get; set; }
        public string? LogoUrl { get; set; }
        // Связь с турниром
        public long TournamentId { get; set; }
        public virtual Tournament Tournament { get; set; } = null!;

        public Gender GenderTeam { get; set; } = Gender.Unknown;
        // Состав команды
        public virtual ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

        // Результаты команды
        public virtual ICollection<TeamResult> Results { get; set; } = new List<TeamResult>();

        // Вычисляемые свойства
        [NotMapped]
        public int Size => Members.Count;

        [NotMapped]
        public decimal AverageRating => Members.Any() ?
            (decimal)Members.Average(m => m.Player?.PlayerRating?.Rating ?? 1500) : 1500m;

        [NotMapped]
        public Player? Captain => Members.FirstOrDefault(m => m.IsCaptain)?.Player;

        [NotMapped]
        public string MemberNames => string.Join(", ", Members.Select(m => m.Player?.FullName));



    }
}
