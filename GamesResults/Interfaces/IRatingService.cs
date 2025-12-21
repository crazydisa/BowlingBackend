using GamesResults.Models.Bowling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamesResults.Interfaces
{
    public interface IRatingService
    {
        /// <summary>
        /// Обновить рейтинги после турнира
        /// </summary>
        Task UpdateRatingsAfterTournamentAsync(long tournamentId);

        /// <summary>
        /// Получить рейтинг игрока
        /// </summary>
        Task<PlayerRating> GetPlayerRatingAsync(long playerId);

        /// <summary>
        /// Получить глобальный рейтинг
        /// </summary>
        Task<List<PlayerRating>> GetGlobalRankingsAsync(int topCount = 100);

        /// <summary>
        /// Получить историю рейтинга игрока
        /// </summary>
        Task<List<RatingHistory>> GetPlayerRatingHistoryAsync(long playerId, int limit = 50);

        /// <summary>
        /// Пересчитать все рейтинги
        /// </summary>
        Task RecalculateAllRatingsAsync();

        /// <summary>
        /// Прогноз результата матча между двумя игроками
        /// </summary>
        Task<MatchPrediction> PredictMatchAsync(long player1Id, long player2Id, TournamentType tournamentType);

        /// <summary>
        /// Получить рейтинг игрока на определенную дату
        /// </summary>
        Task<int> GetPlayerRatingOnDateAsync(long playerId, DateTime date);
    }
}
