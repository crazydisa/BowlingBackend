using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;

namespace GamesResults.Models.Bowling
{
    // Командный результат (наследуется от BaseTournamentResult)
    public class TeamResult : BaseTournamentResult
    {
        // Связь с командой
        public long TeamId { get; set; }
        public virtual Team Team { get; set; } = null!;

        // Детализация по участникам команды (JSON)
        public string MemberScoresJson { get; set; } = "{}"; // { "PlayerId": totalScore, ... }

        // Переопределение абстрактных свойств
        [NotMapped]
        public override string ParticipantName => Team?.Name ?? "Неизвестная команда";

        [NotMapped]
        public override long ParticipantId => TeamId;

        // Вычисляемые свойства
        [NotMapped]
        public Dictionary<int, int> MemberScores
        {
            get => JsonSerializer.Deserialize<Dictionary<int, int>>(MemberScoresJson)
                   ?? new Dictionary<int, int>();
            set => MemberScoresJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public int TeamSize => MemberScores.Count;

        [NotMapped]
        public decimal AveragePerMember => TeamSize > 0 ? TotalScore / TeamSize : 0;
    }
}
