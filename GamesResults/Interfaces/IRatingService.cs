using GamesResults.Models.Bowling;
using System;
using System.Collections.Generic;
using System.Text;

namespace GamesResults.Interfaces
{
    public interface IRatingService
    {
        Task UpdateRatingsAfterTournamentAsync(long tournamentId);
        Task<PlayerRating> GetPlayerRatingAsync(long playerId);
        Task<List<PlayerRating>> GetGlobalRankingsAsync(int topCount = 100);
        Task RecalculateAllRatingsAsync();
    }
}
