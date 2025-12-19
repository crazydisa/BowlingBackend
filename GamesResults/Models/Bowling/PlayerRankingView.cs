using System;
using System.Collections.Generic;
using System.Text;

namespace GamesResults.Models.Bowling
{
    // SQL View для рейтинга
    public class PlayerRankingView
    {
        public long PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int Rank { get; set; }
        public int TournamentCount { get; set; }
        public double AveragePlace { get; set; }
        public string RatingCategory { get; set; } = string.Empty;
        public DateTime LastTournamentDate { get; set; }
    }
}
