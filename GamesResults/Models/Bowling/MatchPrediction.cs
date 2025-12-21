using System;
using System.Collections.Generic;
using System.Text;

namespace GamesResults.Models.Bowling
{
    public class MatchPrediction
    {
        public long Player1Id { get; set; }
        public long Player2Id { get; set; }
        public string Player1Name { get; set; } = string.Empty;
        public string Player2Name { get; set; } = string.Empty;
        public double Player1WinProbability { get; set; }
        public double Player2WinProbability { get; set; }
        public int ExpectedPlayer1RatingChange { get; set; }
        public int ExpectedPlayer2RatingChange { get; set; }
        public string SuggestedWinner { get; set; } = string.Empty;
    }
}
