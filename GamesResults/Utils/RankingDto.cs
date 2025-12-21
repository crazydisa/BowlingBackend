using GamesResults.Models.Bowling;
using System;
using System.Collections.Generic;
using System.Text;

namespace GamesResults.Utils
{
    public class RankingDto
    {
        public int Rank { get; set; }
        public long PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int TournamentCount { get; set; }
        public double AveragePlace { get; set; }
        public string RatingCategory { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
        public int RatingChange { get; set; } // Изменение за последний период

        public static RankingDto FromRating(PlayerRating rating, int rank)
        {
            return new RankingDto
            {
                Rank = rank,
                PlayerId = rating.PlayerId,
                PlayerName = rating.Player?.Name ?? "",
                Rating = rating.Rating,
                TournamentCount = rating.TournamentCount,
                AveragePlace = Math.Round(rating.AveragePlace, 1),
                RatingCategory = rating.RatingCategory,
                AverageScore = Math.Round(rating.AverageScore, 2)
            };
        }
    }
}
