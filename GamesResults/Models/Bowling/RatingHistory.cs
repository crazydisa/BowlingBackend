using System.ComponentModel.DataAnnotations.Schema;

namespace GamesResults.Models.Bowling
{
    // История изменений рейтинга
    public class RatingHistory: Object
    {
        //Связи
        [ForeignKey("PlayerRating")]
        public long PlayerRatingId { get; set; }

        [ForeignKey("Tournament")]
        public long TournamentId { get; set; }

        public int OldRating { get; set; }
        public int NewRating { get; set; }
        public int RatingChange { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
        public string ChangeReason { get; set; } = string.Empty; // "Tournament", "Manual", "Correction"

        // Навигационные свойства
        public virtual PlayerRating PlayerRating { get; set; } = null!;
        public virtual Tournament Tournament { get; set; } = null!;
    }
}
