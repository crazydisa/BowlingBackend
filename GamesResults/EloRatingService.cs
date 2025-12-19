using GamesResults.Interfaces;
using GamesResults.Models.Bowling;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace GamesResults
{
    public class EloRatingService : IRatingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EloRatingService> _logger;

        // Константы системы Эло
        private const int InitialRating = 1500;
        private const int KFactor = 32; // Для опытных игроков можно уменьшить
        private const int KFactorNewPlayer = 40; // Для новых игроков
        private const int GamesForEstablished = 30; // Количество игр для "установившегося" рейтинга

        public EloRatingService(AppDbContext context, ILogger<EloRatingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateRatingsAfterTournamentAsync(long tournamentId)
        {
            try
            {
                _logger.LogInformation("Начало обновления рейтингов для турнира {TournamentId}", tournamentId);

                // Получаем результаты турнира
                var results = await _context.Results
                    .Where(r => r.TournamentId == tournamentId && !r.IsTeam)
                    .OrderBy(r => r.Place)
                    .ToListAsync();

                if (!results.Any())
                {
                    _logger.LogWarning("Нет индивидуальных результатов для турнира {TournamentId}", tournamentId);
                    return;
                }

                _logger.LogInformation("Найдено {Count} результатов для обновления", results.Count);

                // Рассчитываем изменения для всех пар
                for (int i = 0; i < results.Count; i++)
                {
                    for (int j = i + 1; j < results.Count; j++)
                    {
                        var player1 = results[i];
                        var player2 = results[j];

                        // АСИНХРОННО рассчитываем изменения
                        var (change1, change2) = await CalculateEloChangeAsync(
                            player1.PlayerId,
                            player2.PlayerId,
                            player1.Place < player2.Place);

                        // Применяем изменения
                        await UpdatePlayerRatingAsync(player1.PlayerId, change1, tournamentId, "Tournament");
                        await UpdatePlayerRatingAsync(player2.PlayerId, change2, tournamentId, "Tournament");
                    }
                }

                // Обновляем общую статистику
                await UpdatePlayerStatisticsAsync(results);

                // Отмечаем турнир как обработанный
                var tournament = await _context.Tournaments.FindAsync(tournamentId);
                if (tournament != null)
                {
                    tournament.RatingsUpdated = true;
                    tournament.RatingsUpdatedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Рейтинги успешно обновлены для турнира {TournamentId}", tournamentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении рейтингов для турнира {TournamentId}", tournamentId);
                throw;
            }
        }

        private async Task<(int change1, int change2)> CalculateEloChangeAsync(long player1Id, long player2Id, bool player1Won)
        {
            // АСИНХРОННО получаем текущие рейтинги
            var rating1 = await GetPlayerRatingValueAsync(player1Id);
            var rating2 = await GetPlayerRatingValueAsync(player2Id);

            // Безопасный расчет (чтобы избежать переполнения)
            double diff = (rating2 - rating1) / 400.0;

            // Ограничиваем разницу для избежания переполнения
            if (diff > 10) diff = 10;
            if (diff < -10) diff = -10;

            // Рассчитываем ожидаемый результат
            double expected1 = 1.0 / (1.0 + Math.Pow(10, diff));
            double expected2 = 1.0 - expected1;

            // Фактический результат
            double actual1 = player1Won ? 1.0 : 0.0;
            double actual2 = player1Won ? 0.0 : 1.0;

            // Определяем K-фактор
            int k1 = await GetKFactorAsync(player1Id);
            int k2 = await GetKFactorAsync(player2Id);

            // Рассчитываем изменение
            int change1 = (int)(k1 * (actual1 - expected1));
            int change2 = (int)(k2 * (actual2 - expected2));

            // Ограничиваем максимальное изменение
            change1 = Math.Max(-50, Math.Min(50, change1));
            change2 = Math.Max(-50, Math.Min(50, change2));

            return (change1, change2);
        }

        private async Task<int> GetPlayerRatingValueAsync(long playerId)
        {
            var rating = await _context.PlayerRatings
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            return rating?.Rating ?? InitialRating; // Возвращаем InitialRating если рейтинга нет
        }

        private async Task<int> GetKFactorAsync(long playerId)
        {
            // Количество сыгранных турниров
            var gamesCount = await _context.Results
                .CountAsync(r => r.PlayerId == playerId);

            return gamesCount < GamesForEstablished ? KFactorNewPlayer : KFactor;
        }

        private async Task UpdatePlayerRatingAsync(long playerId, int change, long tournamentId, string reason)
        {
            var playerRating = await _context.PlayerRatings
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (playerRating == null)
            {
                playerRating = new PlayerRating
                {
                    PlayerId = playerId,
                    Rating = InitialRating,
                    PeakRating = InitialRating,
                    TournamentCount = 0,
                    AveragePlace = 0,
                    AverageScore = 0,
                    Top3Percentage = 0,
                    Top10Percentage = 0,
                    TotalGames = 0,
                    TotalPins = 0,
                    LastUpdated = DateTime.UtcNow
                };
                _context.PlayerRatings.Add(playerRating);
            }

            var oldRating = playerRating.Rating;
            playerRating.Rating += change;

            if (playerRating.Rating > playerRating.PeakRating)
                playerRating.PeakRating = playerRating.Rating;

            playerRating.LastUpdated = DateTime.UtcNow;

            // Сохраняем историю
            var history = new RatingHistory
            {
                PlayerRatingId = playerRating.Id,
                TournamentId = tournamentId,
                OldRating = oldRating,
                NewRating = playerRating.Rating,
                RatingChange = change,
                ChangeReason = reason,
                ChangeDate = DateTime.UtcNow
            };

            _context.RatingHistories.Add(history);

            _logger.LogDebug("Рейтинг игрока {PlayerId} изменен: {OldRating} → {NewRating} ({Change})",
                playerId, oldRating, playerRating.Rating, change);
        }

        private async Task UpdatePlayerStatisticsAsync(List<TournamentResult> results)
        {
            foreach (var result in results)
            {
                var rating = await _context.PlayerRatings
                    .FirstOrDefaultAsync(r => r.PlayerId == result.PlayerId);

                if (rating == null) continue;

                // Обновляем статистику
                rating.TournamentCount++;
                rating.TotalGames += result.PlayedGamesCount;
                rating.TotalPins += result.Total;

                // Пересчитываем средние значения
                rating.AveragePlace = ((rating.AveragePlace * (rating.TournamentCount - 1)) + result.Place) / rating.TournamentCount;
                rating.AverageScore = ((rating.AverageScore * (rating.TournamentCount - 1)) + result.Average) / rating.TournamentCount;

                // Процент попадания в топ
                if (result.Place <= 3) rating.Top3Percentage = (rating.Top3Percentage * (rating.TournamentCount - 1) + 100) / rating.TournamentCount;
                if (result.Place <= 10) rating.Top10Percentage = (rating.Top10Percentage * (rating.TournamentCount - 1) + 100) / rating.TournamentCount;
            }
        }

        public async Task<PlayerRating> GetPlayerRatingAsync(long playerId)
        {
            var rating = await _context.PlayerRatings
                .Include(r => r.Player)
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (rating == null)
            {
                // Создаем начальный рейтинг, если его нет
                var player = await _context.Players.FindAsync(playerId);
                if (player == null)
                    throw new ArgumentException($"Игрок {playerId} не найден");

                rating = new PlayerRating
                {
                    PlayerId = playerId,
                    Player = player,
                    Rating = InitialRating,
                    PeakRating = InitialRating
                };

                _context.PlayerRatings.Add(rating);
                await _context.SaveChangesAsync();
            }

            return rating;
        }

        public async Task<List<PlayerRating>> GetGlobalRankingsAsync(int topCount = 100)
        {
            return await _context.PlayerRatings
                .Include(r => r.Player)
                .Where(r => r.TournamentCount >= 3) // Минимум 3 турнира для попадания в рейтинг
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.TournamentCount)
                .ThenBy(r => r.AveragePlace)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task RecalculateAllRatingsAsync()
        {
            _logger.LogInformation("Начало перерасчета всех рейтингов");

            // Очищаем текущие рейтинги
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Bowling.RatingHistories");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Bowling.PlayerRatings");

            // Получаем все турниры по дате
            var tournaments = await _context.Tournaments
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            // Пересчитываем рейтинги для каждого турнира по порядку
            foreach (var tournament in tournaments)
            {
                await UpdateRatingsAfterTournamentAsync(tournament.Id);
            }

            _logger.LogInformation("Перерасчет всех рейтингов завершен");
        }
    }
}
