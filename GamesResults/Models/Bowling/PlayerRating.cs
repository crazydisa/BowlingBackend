
using System.ComponentModel.DataAnnotations.Schema;

namespace GamesResults.Models.Bowling
{
    // Рейтинг игрока (система Эло)
    public class PlayerRating: Object
    {
        [ObjectPropertyAttribute(TypeName = "Player", DisplayExpr = "Name", NameSpace = "GamesResults.Models.Bowling")]
        [ForeignKey("Player")]
        public long PlayerId { get; set; }

        // Текущий рейтинг (например, Эло)
        public int Rating { get; set; } = 1500; // Начальный рейтинг

        // Максимальный достигнутый рейтинг
        public int PeakRating { get; set; } = 1500;

        //Статистика
        // Количество турниров
        public int TournamentCount { get; set; }

        // Среднее место
        public double AveragePlace { get; set; }

        // Процент попадания в топ-N
        public double Top3Percentage { get; set; }
        public double Top10Percentage { get; set; }

        // Общая статистика
        public int TotalGames { get; set; }
        public int TotalPins { get; set; } // Общее количество кеглей
        public decimal AverageScore { get; set; }

        // Дата последнего обновления
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Навигационное свойство
        public virtual Player Player { get; set; } = null!;
        // Навигационное свойство для истории изменений
        public virtual ICollection<RatingHistory> History { get; set; } = new List<RatingHistory>();
        // Вычисляемые свойства
        [NotMapped]
        public double AveragePinsPerGame => TotalGames > 0 ? (double)TotalPins / TotalGames : 0;

        [NotMapped]
        public string RatingCategory => Rating switch
        {
            >= 2000 => "Мастер",
            >= 1800 => "Эксперт",
            >= 1600 => "Продвинутый",
            >= 1400 => "Средний",
            _ => "Начинающий"
        };
    }
}
