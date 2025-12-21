using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GamesResults.Models.Bowling
{
    // Индивидуальный результат (наследуется от BaseTournamentResult)
    public class IndividualResult : BaseTournamentResult
    {
        // Связь с игроком
        public long PlayerId { get; set; }
        public virtual Player Player { get; set; } = null!;

        // Дополнительная статистика для индивида
        public int HighGame { get; set; }      // Лучшая игра
        public int LowGame { get; set; }       // Худшая игра
        public int StrikeCount { get; set; }   // Количество страйков
        public int SpareCount { get; set; }    // Количество спэров

        // Переопределение абстрактных свойств
        [NotMapped]
        public override string ParticipantName => Player?.FullName ?? "Неизвестный игрок";

        [NotMapped]
        public override long ParticipantId => PlayerId;

        // Вычисляемые свойства
        [NotMapped]
        public decimal Consistency => GamesPlayed > 0 ?
            (decimal)(HighGame - LowGame) / GamesPlayed : 0;
    }
}
