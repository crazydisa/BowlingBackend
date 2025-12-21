using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;

namespace GamesResults.Models.Bowling
{
    public abstract class BaseTournamentResult: Object
    {
        // Общие для всех результатов поля
        public long TournamentId { get; set; }
        public virtual Tournament Tournament { get; set; } = null!;

        public int Place { get; set; }           // Финальное место
        public decimal TotalScore { get; set; }  // Сумма очков
        public decimal AverageScore { get; set; } // Средний результат
        public int GamesPlayed { get; set; }     // Количество сыгранных игр



        // Детализация по играм (хранится как JSON или в связанной таблице)
        public string GameScoresJson { get; set; } = "[]"; // JSON: [220, 180, 195, ...]

        // Метаданные
        public DateTime ResultDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        // Детализация по играм
        public int Game1 { get; set; }
        public int Game2 { get; set; }
        public int Game3 { get; set; }
        public int Game4 { get; set; }
        public int Game5 { get; set; }
        public int Game6 { get; set; }
        // Вычисляемые свойства
        [NotMapped]
        public List<int> GameScores
        {
            get => JsonSerializer.Deserialize<List<int>>(GameScoresJson) ?? new List<int>();
            set => GameScoresJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public bool IsValid => GamesPlayed > 0 && TotalScore > 0;

        // Абстрактное свойство для получения имени участника/команды
        [NotMapped]
        public abstract string ParticipantName { get; }

        // Абстрактное свойство для получения ID участника/команды
        [NotMapped]
        public abstract long ParticipantId { get; }

        // Методы
        public int CalculateTotal() => Game1 + Game2 + Game3 + Game4 + Game5 + Game6;

        public double CalculateAverage()
        {
            int total = CalculateTotal();
            int gamesCount = GetAllGameResults().Count(score => score > 0);
            return gamesCount > 0 ? Math.Round((double)total / gamesCount, 2) : 0;
        }

        public int[] GetAllGameResults() => new[] { Game1, Game2, Game3, Game4, Game5, Game6 };

        public string GetGamesSummary() => $"{Game1}/{Game2}/{Game3}/{Game4}/{Game5}/{Game6}";
    }
}
