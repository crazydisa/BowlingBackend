using GamesResults.Interfaces;
using GamesResults.Models.Bowling;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
namespace GamesResults
{
    public class EloRatingService : IRatingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EloRatingService> _logger;

        // Константы системы Эло
        private const int INITIAL_RATING = 1500;
        private const int K_FACTOR_BASE = 32;
        private const int K_FACTOR_NEW_PLAYER = 40;
        private const int K_FACTOR_EXPERIENCED = 24;
        private const int MIN_TOURNAMENTS_FOR_EXPERIENCED = 20;

        // Веса турниров в зависимости от типа
        private static readonly Dictionary<TournamentType, double> TournamentWeights = new()
        {
            [TournamentType.Individual] = 1.0,
            [TournamentType.Team] = 0.8,     // Командные турниры влияют меньше
            [TournamentType.Mixed] = 1.2,    // Смешанные турниры важнее
            [TournamentType.Unknown] = 0.5   // Неизвестные турниры влияют меньше
        };

        // Веса в зависимости от места
        private static readonly Dictionary<int, double> PlaceWeights = new()
        {
            [1] = 1.5,   // Победитель
            [2] = 1.2,   // Финалист
            [3] = 1.1,   // Бронза
            [4] = 1.0,   // Полуфиналисты
            [5] = 0.9,   // и т.д.
            [6] = 0.8,
            [7] = 0.7,
            [8] = 0.6,
            [9] = 0.5,
            [10] = 0.4
        };

        public EloRatingService(AppDbContext context, ILogger<EloRatingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Основной метод обновления рейтингов после турнира
        /// </summary>
        public async Task UpdateRatingsAfterTournamentAsync(long tournamentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Начало обновления рейтингов для турнира {TournamentId}", tournamentId);

                // Получаем турнир
                var tournament = await _context.Tournaments
                    .Include(t => t.Results)
                        .ThenInclude(r => ((IndividualResult)r).Player)
                    .FirstOrDefaultAsync(t => t.Id == tournamentId);

                if (tournament == null)
                {
                    _logger.LogWarning("Турнир {TournamentId} не найден", tournamentId);
                    throw new ArgumentException($"Турнир с ID {tournamentId} не найден");
                }

                // Проверяем, не обновлялись ли рейтинги уже
                if (tournament.RatingsUpdated)
                {
                    _logger.LogWarning("Рейтинги для турнира {TournamentId} уже были обновлены", tournamentId);
                    return;
                }

                // Получаем индивидуальные результаты турнира
                var individualResults = tournament.Results
                    .OfType<IndividualResult>()
                    .OrderBy(r => r.Place)
                    .ToList();

                if (!individualResults.Any())
                {
                    _logger.LogWarning("Нет индивидуальных результатов для турнира {TournamentId}", tournamentId);
                    return;
                }

                _logger.LogInformation("Найдено {Count} индивидуальных результатов", individualResults.Count);

                // Рассчитываем изменения рейтингов для всех пар участников
                var ratingChanges = new Dictionary<long, int>();

                // Метод 1: Система сравнения всех со всеми (подходит для небольших турниров)
                if (individualResults.Count <= 50)
                {
                    ratingChanges = await CalculateRatingChangesAllVsAllAsync(individualResults, tournament.TournamentType);
                }
                // Метод 2: Система сравнения по местам (для больших турниров)
                else
                {
                    ratingChanges = await CalculateRatingChangesByPlaceAsync(individualResults, tournament.TournamentType);
                }

                // Применяем изменения рейтингов
                await ApplyRatingChangesAsync(ratingChanges, tournamentId);

                // Обновляем статистику игроков
                await UpdatePlayerStatisticsAsync(individualResults);

                // Отмечаем турнир как обработанный
                tournament.RatingsUpdated = true;
                tournament.RatingsUpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Рейтинги успешно обновлены для турнира {TournamentId}. Изменено {Count} игроков",
                    tournamentId, ratingChanges.Count);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при обновлении рейтингов для турнира {TournamentId}", tournamentId);
                throw;
            }
        }

        /// <summary>
        /// Метод сравнения всех участников со всеми
        /// </summary>
        private async Task<Dictionary<long, int>> CalculateRatingChangesAllVsAllAsync(
            List<IndividualResult> results,
            TournamentType tournamentType)
        {
            var ratingChanges = new Dictionary<long, int>();

            for (int i = 0; i < results.Count; i++)
            {
                for (int j = i + 1; j < results.Count; j++)
                {
                    var player1Result = results[i];
                    var player2Result = results[j];

                    // Определяем победителя (меньшее место = лучше)
                    bool player1Won = player1Result.Place < player2Result.Place;

                    // Получаем текущие рейтинги
                    var rating1 = await GetPlayerRatingValueAsync(player1Result.PlayerId);
                    var rating2 = await GetPlayerRatingValueAsync(player2Result.PlayerId);

                    // Определяем вес матча в зависимости от разницы мест
                    double matchWeight = CalculateMatchWeight(player1Result.Place, player2Result.Place);

                    // Рассчитываем изменение рейтинга
                    var (change1, change2) = CalculateEloChange(
                        rating1,
                        rating2,
                        player1Won,
                        player1Result.PlayerId,
                        player2Result.PlayerId,
                        tournamentType,
                        matchWeight);

                    // Добавляем изменения в словарь
                    if (!ratingChanges.ContainsKey(player1Result.PlayerId))
                        ratingChanges[player1Result.PlayerId] = 0;
                    if (!ratingChanges.ContainsKey(player2Result.PlayerId))
                        ratingChanges[player2Result.PlayerId] = 0;

                    ratingChanges[player1Result.PlayerId] += change1;
                    ratingChanges[player2Result.PlayerId] += change2;
                }
            }

            return ratingChanges;
        }

        /// <summary>
        /// Метод сравнения по местам (для больших турниров)
        /// </summary>
        private async Task<Dictionary<long, int>> CalculateRatingChangesByPlaceAsync(
            List<IndividualResult> results,
            TournamentType tournamentType)
        {
            var ratingChanges = new Dictionary<long, int>();

            // Разбиваем участников на группы по местам
            var groupedResults = results
                .Select((r, index) => new { Result = r, GroupIndex = index / 10 }) // Группы по 10 человек
                .GroupBy(x => x.GroupIndex)
                .Select(g => g.Select(x => x.Result).ToList())
                .ToList();

            for (int groupIndex = 0; groupIndex < groupedResults.Count; groupIndex++)
            {
                var group = groupedResults[groupIndex];

                // Внутри группы считаем всех со всеми
                for (int i = 0; i < group.Count; i++)
                {
                    for (int j = i + 1; j < group.Count; j++)
                    {
                        var player1Result = group[i];
                        var player2Result = group[j];

                        bool player1Won = player1Result.Place < player2Result.Place;

                        var rating1 = await GetPlayerRatingValueAsync(player1Result.PlayerId);
                        var rating2 = await GetPlayerRatingValueAsync(player2Result.PlayerId);

                        // Вес матча внутри группы выше
                        double matchWeight = 1.0;

                        var (change1, change2) = CalculateEloChange(
                            rating1, rating2, player1Won,
                            player1Result.PlayerId, player2Result.PlayerId,
                            tournamentType, matchWeight);

                        AddRatingChange(ratingChanges, player1Result.PlayerId, change1);
                        AddRatingChange(ratingChanges, player2Result.PlayerId, change2);
                    }
                }

                // Сравниваем с участниками из следующей группы
                if (groupIndex < groupedResults.Count - 1)
                {
                    var nextGroup = groupedResults[groupIndex + 1];

                    foreach (var player1Result in group)
                    {
                        foreach (var player2Result in nextGroup)
                        {
                            // Игрок из текущей группы всегда побеждает игрока из следующей
                            var rating1 = await GetPlayerRatingValueAsync(player1Result.PlayerId);
                            var rating2 = await GetPlayerRatingValueAsync(player2Result.PlayerId);

                            // Вес матча между группами меньше
                            double matchWeight = 0.5;

                            var (change1, change2) = CalculateEloChange(
                                rating1, rating2, true,
                                player1Result.PlayerId, player2Result.PlayerId,
                                tournamentType, matchWeight);

                            AddRatingChange(ratingChanges, player1Result.PlayerId, change1);
                            AddRatingChange(ratingChanges, player2Result.PlayerId, change2);
                        }
                    }
                }
            }

            return ratingChanges;
        }

        /// <summary>
        /// Основной расчет изменения рейтинга по системе Эло
        /// </summary>
        private (int change1, int change2) CalculateEloChange(
            long rating1,
            long rating2,
            bool player1Won,
            long player1Id,
            long player2Id,
            TournamentType tournamentType,
            double matchWeight = 1.0)
        {
            // Рассчитываем ожидаемый результат
            double expected1 = CalculateExpectedScore(rating1, rating2);
            double expected2 = 1.0 - expected1;

            // Фактический результат
            double actual1 = player1Won ? 1.0 : 0.0;
            double actual2 = player1Won ? 0.0 : 1.0;

            // Определяем K-факторы для игроков
            int k1 = GetKFactor(player1Id);
            int k2 = GetKFactor(player2Id);

            // Применяем вес турнира
            double tournamentWeight = GetTournamentWeight(tournamentType);

            // Рассчитываем изменения
            double rawChange1 = k1 * (actual1 - expected1) * tournamentWeight * matchWeight;
            double rawChange2 = k2 * (actual2 - expected2) * tournamentWeight * matchWeight;

            // Округляем и ограничиваем изменения
            int change1 = LimitRatingChange((int)Math.Round(rawChange1));
            int change2 = LimitRatingChange((int)Math.Round(rawChange2));

            // Гарантируем, что сумма изменений близка к 0 (система с нулевой суммой)
            int totalChange = change1 + change2;
            if (Math.Abs(totalChange) > 1)
            {
                change1 -= totalChange / 2;
                change2 -= totalChange / 2;
            }

            return (change1, change2);
        }

        /// <summary>
        /// Расчет ожидаемого результата
        /// </summary>
        private double CalculateExpectedScore(long ratingA, long ratingB)
        {
            // Классическая формула Эло
            return 1.0 / (1.0 + Math.Pow(10, (ratingB - ratingA) / 400.0));
        }

        /// <summary>
        /// Определение K-фактора для игрока
        /// </summary>
        private int GetKFactor(long playerId)
        {
            // В реальной реализации нужно получить количество турниров игрока
            // Здесь упрощенная версия - возвращаем базовые значения

            // TODO: Реализовать получение количества турниров игрока из БД
            // var tournamentCount = await GetPlayerTournamentCountAsync(playerId);

            // Временная заглушка
            var tournamentCount = 0;

            if (tournamentCount < 10)
                return K_FACTOR_NEW_PLAYER;
            else if (tournamentCount < MIN_TOURNAMENTS_FOR_EXPERIENCED)
                return K_FACTOR_BASE;
            else
                return K_FACTOR_EXPERIENCED;
        }

        /// <summary>
        /// Вес турнира в зависимости от типа
        /// </summary>
        private double GetTournamentWeight(TournamentType tournamentType)
        {
            return TournamentWeights.TryGetValue(tournamentType, out var weight)
                ? weight
                : 1.0;
        }

        /// <summary>
        /// Вес матча в зависимости от разницы мест
        /// </summary>
        private double CalculateMatchWeight(int place1, int place2)
        {
            int placeDiff = Math.Abs(place1 - place2);

            // Чем больше разница в местах, тем меньше вес матча
            if (placeDiff <= 5) return 1.0;
            if (placeDiff <= 10) return 0.8;
            if (placeDiff <= 20) return 0.6;
            if (placeDiff <= 30) return 0.4;
            return 0.2;
        }

        /// <summary>
        /// Ограничение максимального изменения рейтинга за один матч
        /// </summary>
        private int LimitRatingChange(int change)
        {
            // Максимальное изменение ±50 очков
            return Math.Max(-50, Math.Min(50, change));
        }

        /// <summary>
        /// Получение текущего рейтинга игрока
        /// </summary>
        private async Task<long> GetPlayerRatingValueAsync(long playerId)
        {
            var rating = await _context.PlayerRatings
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            return rating?.Rating ?? INITIAL_RATING;
        }

        /// <summary>
        /// Применение изменений рейтингов
        /// </summary>
        private async Task ApplyRatingChangesAsync(Dictionary<long, int> ratingChanges, long tournamentId)
        {
            foreach (var kvp in ratingChanges)
            {
                var playerId = kvp.Key;
                var change = kvp.Value;

                if (change == 0) continue;

                await UpdatePlayerRatingAsync(playerId, change, tournamentId);
            }
        }

        /// <summary>
        /// Обновление рейтинга конкретного игрока
        /// </summary>
        private async Task UpdatePlayerRatingAsync(long playerId, int change, long tournamentId)
        {
            var playerRating = await _context.PlayerRatings
                .Include(pr => pr.Player)
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (playerRating == null)
            {
                // Создаем новый рейтинг, если его нет
                var player = await _context.Players.FindAsync(playerId);
                if (player == null)
                {
                    _logger.LogWarning("Игрок {PlayerId} не найден", playerId);
                    return;
                }

                playerRating = new PlayerRating
                {
                    PlayerId = playerId,
                    Player = player,
                    Rating = INITIAL_RATING,
                    PeakRating = INITIAL_RATING,
                    TournamentCount = 0,
                    AveragePlace = 0,
                    AverageScore = 0,
                    TotalGames = 0,
                    TotalPins = 0,
                    Top3Percentage = 0,
                    Top10Percentage = 0,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PlayerRatings.Add(playerRating);
                await _context.SaveChangesAsync(); // Сохраняем, чтобы получить Id
            }

            var oldRating = playerRating.Rating;
            var newRating = playerRating.Rating + change;

            // Обновляем рейтинг
            playerRating.Rating = newRating;

            // Обновляем пиковый рейтинг
            if (newRating > playerRating.PeakRating)
                playerRating.PeakRating = newRating;

            playerRating.LastUpdated = DateTime.UtcNow;

            // Сохраняем историю изменений
            var history = new RatingHistory
            {
                PlayerRatingId = playerRating.Id,
                TournamentId = tournamentId,
                OldRating = oldRating,
                NewRating = newRating,
                RatingChange = change,
                ChangeReason = "Tournament",
                ChangeDate = DateTime.UtcNow
            };

            _context.RatingHistories.Add(history);

            _logger.LogDebug("Рейтинг игрока {PlayerId} ({PlayerName}): {OldRating} → {NewRating} ({Change:+0;-0;0})",
                playerId, playerRating.Player?.Name ?? "Unknown", oldRating, newRating, change);
        }

        /// <summary>
        /// Обновление статистики игроков
        /// </summary>
        private async Task UpdatePlayerStatisticsAsync(List<IndividualResult> results)
        {
            foreach (var result in results)
            {
                var rating = await _context.PlayerRatings
                    .FirstOrDefaultAsync(r => r.PlayerId == result.PlayerId);

                if (rating == null) continue;

                // Обновляем счетчик турниров
                rating.TournamentCount++;

                // Обновляем общее количество игр и очков
                rating.TotalGames += result.GamesPlayed;
                rating.TotalPins += (int)result.TotalScore;

                // Пересчитываем средние значения
                rating.AveragePlace = ((rating.AveragePlace * (rating.TournamentCount - 1)) + result.Place) / rating.TournamentCount;
                decimal previousTotal = rating.AverageScore * (rating.TournamentCount - 1);
                decimal newTotal = previousTotal + result.AverageScore;
                rating.AverageScore = newTotal / rating.TournamentCount;


                // Обновляем процент попадания в топ
                if (result.Place <= 3)
                {
                    rating.Top3Percentage = ((rating.Top3Percentage * (rating.TournamentCount - 1)) + 100) / rating.TournamentCount;
                }
                else
                {
                    rating.Top3Percentage = (rating.Top3Percentage * (rating.TournamentCount - 1)) / rating.TournamentCount;
                }

                if (result.Place <= 10)
                {
                    rating.Top10Percentage = ((rating.Top10Percentage * (rating.TournamentCount - 1)) + 100) / rating.TournamentCount;
                }
                else
                {
                    rating.Top10Percentage = (rating.Top10Percentage * (rating.TournamentCount - 1)) / rating.TournamentCount;
                }
            }
        }

        /// <summary>
        /// Вспомогательный метод для добавления изменения рейтинга
        /// </summary>
        private void AddRatingChange(Dictionary<long, int> ratingChanges, long playerId, int change)
        {
            if (!ratingChanges.ContainsKey(playerId))
                ratingChanges[playerId] = 0;

            ratingChanges[playerId] += change;
        }

        /// <summary>
        /// Получить рейтинг игрока
        /// </summary>
        public async Task<PlayerRating> GetPlayerRatingAsync(long playerId)
        {
            var rating = await _context.PlayerRatings
                .Include(r => r.Player)
                .Include(r => r.History)
                    .ThenInclude(h => h.Tournament)
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (rating == null)
            {
                // Создаем начальный рейтинг, если его нет
                var player = await _context.Players.FindAsync(playerId);
                if (player == null)
                    throw new ArgumentException($"Игрок с ID {playerId} не найден");

                rating = new PlayerRating
                {
                    PlayerId = playerId,
                    Player = player,
                    Rating = INITIAL_RATING,
                    PeakRating = INITIAL_RATING,
                    TournamentCount = 0,
                    LastUpdated = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PlayerRatings.Add(rating);
                await _context.SaveChangesAsync();

                // Загружаем связанные данные
                rating = await _context.PlayerRatings
                    .Include(r => r.Player)
                    .Include(r => r.History)
                    .FirstOrDefaultAsync(r => r.PlayerId == playerId);
            }

            return rating;
        }

        /// <summary>
        /// Получить глобальный рейтинг
        /// </summary>
        public async Task<List<PlayerRating>> GetGlobalRankingsAsync(int topCount = 100)
        {
            return await _context.PlayerRatings
                .Include(r => r.Player)
                    .ThenInclude(p => p.District)
                .Where(r => r.TournamentCount >= 3) // Минимум 3 турнира для попадания в рейтинг
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.TournamentCount)
                .ThenBy(r => r.AveragePlace)
                .Take(Math.Min(topCount, 500))
                .ToListAsync();
        }

        /// <summary>
        /// Получить историю рейтинга игрока
        /// </summary>
        public async Task<List<RatingHistory>> GetPlayerRatingHistoryAsync(long playerId, int limit = 50)
        {
            var rating = await _context.PlayerRatings
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (rating == null)
                return new List<RatingHistory>();

            return await _context.RatingHistories
                .Include(h => h.Tournament)
                .Where(h => h.PlayerRatingId == rating.Id)
                .OrderByDescending(h => h.ChangeDate)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Пересчитать все рейтинги
        /// </summary>
        public async Task RecalculateAllRatingsAsync()
        {
            _logger.LogInformation("Начало перерасчета всех рейтингов");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Очищаем текущие рейтинги
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Bowling.RatingHistories");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Bowling.PlayerRatings");

                // Сбрасываем флаги обновления у турниров
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Bowling.Tournaments SET RatingsUpdated = 0, RatingsUpdatedDate = NULL");

                // Получаем все турниры по дате
                var tournaments = await _context.Tournaments
                    .Where(t => t.StartDate.HasValue)
                    .OrderBy(t => t.StartDate)
                    .ToListAsync();

                _logger.LogInformation("Найдено {Count} турниров для перерасчета", tournaments.Count);

                // Пересчитываем рейтинги для каждого турнира по порядку
                int processedCount = 0;
                foreach (var tournament in tournaments)
                {
                    try
                    {
                        await UpdateRatingsAfterTournamentAsync(tournament.Id);
                        processedCount++;

                        if (processedCount % 10 == 0)
                        {
                            _logger.LogInformation("Обработано {ProcessedCount}/{TotalCount} турниров",
                                processedCount, tournaments.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при обработке турнира {TournamentId}", tournament.Id);
                        // Продолжаем обработку других турниров
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Перерасчет всех рейтингов завершен. Обработано {ProcessedCount} турниров",
                    processedCount);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка при перерасчете всех рейтингов");
                throw;
            }
        }

        /// <summary>
        /// Прогноз результата матча
        /// </summary>
        public async Task<MatchPrediction> PredictMatchAsync(long player1Id, long player2Id, TournamentType tournamentType)
        {
            var rating1 = await GetPlayerRatingValueAsync(player1Id);
            var rating2 = await GetPlayerRatingValueAsync(player2Id);

            var player1 = await _context.Players.FindAsync(player1Id);
            var player2 = await _context.Players.FindAsync(player2Id);

            double expected1 = CalculateExpectedScore(rating1, rating2);
            double expected2 = 1.0 - expected1;

            // Рассчитываем ожидаемые изменения рейтинга
            int k1 = GetKFactor(player1Id);
            int k2 = GetKFactor(player2Id);
            double tournamentWeight = GetTournamentWeight(tournamentType);

            int expectedChange1Win = (int)(k1 * (1.0 - expected1) * tournamentWeight);
            int expectedChange1Lose = (int)(k1 * (0.0 - expected1) * tournamentWeight);
            int expectedChange2Win = (int)(k2 * (1.0 - expected2) * tournamentWeight);
            int expectedChange2Lose = (int)(k2 * (0.0 - expected2) * tournamentWeight);

            return new MatchPrediction
            {
                Player1Id = player1Id,
                Player2Id = player2Id,
                Player1Name = player1?.Name ?? "Unknown",
                Player2Name = player2?.Name ?? "Unknown",
                Player1WinProbability = Math.Round(expected1 * 100, 1),
                Player2WinProbability = Math.Round(expected2 * 100, 1),
                ExpectedPlayer1RatingChange = expectedChange1Win,
                ExpectedPlayer2RatingChange = expectedChange2Lose,
                SuggestedWinner = expected1 > 0.5 ? player1?.Name : player2?.Name
            };
        }

        /// <summary>
        /// Получить рейтинг игрока на определенную дату
        /// </summary>
        public async Task<int> GetPlayerRatingOnDateAsync(long playerId, DateTime date)
        {
            // Находим последнее изменение рейтинга до указанной даты
            var rating = await _context.PlayerRatings
                .Include(r => r.History)
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (rating == null)
                return INITIAL_RATING;

            var lastChange = rating.History
                .Where(h => h.ChangeDate <= date)
                .OrderByDescending(h => h.ChangeDate)
                .FirstOrDefault();

            return lastChange?.NewRating ?? rating.Rating;
        }

        /// <summary>
        /// Получить количество турниров игрока (вспомогательный метод)
        /// </summary>
        private async Task<int> GetPlayerTournamentCountAsync(int playerId)
        {
            return await _context.IndividualResults
                .Where(r => r.PlayerId == playerId)
                .Select(r => r.TournamentId)
                .Distinct()
                .CountAsync();
        }
    }
}
