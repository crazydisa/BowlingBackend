using GamesResults;
using GamesResults.Interfaces;
using GamesResults.Models.Bowling;
using GamesResults.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
using System.Text.Json.Serialization;


namespace BowlingStatistic.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RatingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RatingsController> _logger;
        private readonly IRatingService _ratingService;

        public RatingsController(
            AppDbContext context,
            ILogger<RatingsController> logger,
            IRatingService ratingService)
        {
            _context = context;
            _logger = logger;
            _ratingService = ratingService;
        }

        #region Основные методы API

        /// <summary>
        /// Получить глобальный рейтинг игроков
        /// GET: api/ratings/global
        /// </summary>
        [HttpGet("global")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<GlobalRankingsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGlobalRankings(
            [FromQuery] RankingsQueryDto query)
        {
            try
            {
                _logger.LogInformation("Запрос глобального рейтинга: {@Query}", query);

                // Базовый запрос
                var baseQuery = _context.PlayerRatings
                    .Include(r => r.Player)
                        .ThenInclude(p => p.District)
                    .AsQueryable();

                // Применяем фильтры
                if (query.OnlyActive)
                {
                    var oneYearAgo = DateTime.UtcNow.AddYears(-1);
                    baseQuery = baseQuery.Where(r => r.LastUpdated >= oneYearAgo);
                }

                if (!string.IsNullOrEmpty(query.Region))
                {
                    baseQuery = baseQuery.Where(r =>
                        r.Player.District != null &&
                        r.Player.District.Name.Contains(query.Region));
                }

                if (query.Gender.HasValue && query.Gender.Value != Gender.Unknown)
                {
                    baseQuery = baseQuery.Where(r => r.Player.Gender == query.Gender.Value);
                }

                if (query.MinTournaments.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.TournamentCount >= query.MinTournaments.Value);
                }

                if (query.MinRating.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.Rating >= query.MinRating.Value);
                }

                if (query.MaxRating.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.Rating <= query.MaxRating.Value);
                }

                // Сортировка
                var orderedQuery = query.SortBy switch
                {
                    "rating" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.Rating)
                        : baseQuery.OrderBy(r => r.Rating),
                    "tournaments" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.TournamentCount)
                        : baseQuery.OrderBy(r => r.TournamentCount),
                    "averageScore" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.AverageScore)
                        : baseQuery.OrderBy(r => r.AverageScore),
                    "averagePlace" => query.Descending
                        ? baseQuery.OrderBy(r => r.AveragePlace)
                        : baseQuery.OrderByDescending(r => r.AveragePlace),
                    _ => baseQuery.OrderByDescending(r => r.Rating)
                };

                // Получаем общее количество для пагинации
                var totalCount = await orderedQuery.CountAsync();

                // Применяем пагинацию
                var players = await orderedQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(r => new PlayerRankingDto
                    {
                        Id = r.PlayerId,
                        FullName = r.Player.FullName,
                        Region = r.Player.District != null ? r.Player.District.Name : null,
                        Gender = r.Player.Gender,
                        Rating = r.Rating,
                        PeakRating = r.PeakRating,
                        TournamentCount = r.TournamentCount,
                        AverageScore = r.AverageScore,
                        AveragePlace = r.AveragePlace,
                        Top3Percentage = r.Top3Percentage,
                        Top10Percentage = r.Top10Percentage,
                        LastUpdated = r.LastUpdated,
                        TotalGames = r.TotalGames,
                        TotalPins = r.TotalPins
                    })
                    .ToListAsync();

                // Добавляем места (позиции в рейтинге)
                var rankedPlayers = players.Select((p, index) =>
                {
                    p.Place = (query.Page - 1) * query.PageSize + index + 1;
                    return p;
                }).ToList();

                var result = new GlobalRankingsDto
                {
                    Players = rankedPlayers,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return Ok(ApiResponse<GlobalRankingsDto>.Success(result, "Рейтинг успешно получен"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении глобального рейтинга");
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить рейтинг конкретного игрока
        /// GET: api/ratings/player/{id}
        /// </summary>
        [HttpGet("player/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PlayerRatingDetailDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayerRating(int id)
        {
            try
            {
                var rating = await _context.PlayerRatings
                    .Include(r => r.Player)
                        .ThenInclude(p => p.District)
                    .FirstOrDefaultAsync(r => r.PlayerId == id);

                if (rating == null)
                {
                    // Получаем или создаем рейтинг
                    rating = await _ratingService.GetPlayerRatingAsync(id);
                }

                var result = new PlayerRatingDetailDto
                {
                    Id = rating.PlayerId,
                    FullName = rating.Player?.FullName ?? "Неизвестный игрок",
                    Region = rating.Player?.District?.Name,
                    Gender = rating.Player?.Gender ?? Gender.Unknown,
                    Rating = rating.Rating,
                    PeakRating = rating.PeakRating,
                    TournamentCount = rating.TournamentCount,
                    AverageScore = rating.AverageScore,
                    AveragePlace = rating.AveragePlace,
                    Top3Percentage = rating.Top3Percentage,
                    Top10Percentage = rating.Top10Percentage,
                    TotalGames = rating.TotalGames,
                    TotalPins = rating.TotalPins,
                    LastUpdated = rating.LastUpdated,
                    Created = rating.CreatedAt
                };

                // Рассчитываем дополнительные метрики
                result.Consistency = CalculateConsistency(rating);
                result.Streak = await CalculateCurrentStreakAsync(id);
                result.Place = await GetPlayerGlobalPlaceAsync(id);
                result.RatingChangeLastMonth = await GetRatingChangeAsync(id, DateTime.UtcNow.AddMonths(-1));

                return Ok(ApiResponse<PlayerRatingDetailDto>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении рейтинга игрока {PlayerId}", id);
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить статистику рейтинговой системы
        /// GET: api/ratings/statistics
        /// </summary>
        [HttpGet("statistics")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<RatingStatisticsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRatingStatistics()
        {
            try
            {
                var statistics = new RatingStatisticsDto
                {
                    // Основные показатели
                    TotalPlayers = await _context.PlayerRatings.CountAsync(),
                    ActivePlayers = await _context.PlayerRatings
                        .Where(r => r.LastUpdated >= DateTime.UtcNow.AddMonths(-6))
                        .CountAsync(),
                    NewPlayersLastMonth = await _context.PlayerRatings
                        .Where(r => r.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                        .CountAsync(),

                    // Рейтинговые показатели
                    AverageRating = await _context.PlayerRatings
                        .Where(r => r.TournamentCount >= 3)
                        .AverageAsync(r => (double?)r.Rating) ?? 1500,
                    HighestRating = await _context.PlayerRatings
                        .MaxAsync(r => (int?)r.Rating) ?? 1500,
                    LowestRating = await _context.PlayerRatings
                        .Where(r => r.TournamentCount >= 3)
                        .MinAsync(r => (int?)r.Rating) ?? 1500,
                    MedianRating = await CalculateMedianRatingAsync(),

                    // Статистика турниров
                    TournamentsProcessed = await _context.Tournaments
                        .CountAsync(t => t.RatingsUpdated),
                    AverageTournamentsPerPlayer = await _context.PlayerRatings
                        .Where(r => r.TournamentCount > 0)
                        .AverageAsync(r => (double?)r.TournamentCount) ?? 0,

                    // Даты
                    LastUpdated = await _context.PlayerRatings
                        .MaxAsync(r => (DateTime?)r.LastUpdated) ?? DateTime.MinValue,
                    OldestRatingDate = await _context.PlayerRatings
                        .MinAsync(r => (DateTime?)r.CreatedAt) ?? DateTime.MinValue
                };

                // Распределение по рейтинговым диапазонам
                statistics.RatingDistribution = await GetRatingDistributionAsync();

                // Распределение по количеству турниров
                statistics.TournamentDistribution = await GetTournamentDistributionAsync();

                // Топ регионов
                statistics.TopRegions = await GetTopRegionsAsync(10);

                // Статистика по полу
                statistics.GenderStatistics = await GetGenderStatisticsAsync();

                return Ok(ApiResponse<RatingStatisticsDto>.Success(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики рейтингов");
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить историю рейтинга игрока
        /// GET: api/ratings/player/{id}/history
        /// </summary>
        [HttpGet("player/{id}/history")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<RatingHistoryListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayerRatingHistory(
            int id,
            [FromQuery] RatingHistoryQueryDto query)
        {
            try
            {
                // Проверяем существование игрока
                var playerExists = await _context.Players.AnyAsync(p => p.Id == id);
                if (!playerExists)
                {
                    return NotFound(ApiResponse.Error($"Игрок с ID {id} не найден"));
                }

                var rating = await _context.PlayerRatings
                    .FirstOrDefaultAsync(r => r.PlayerId == id);

                if (rating == null)
                {
                    return Ok(ApiResponse<RatingHistoryListDto>.Success(new RatingHistoryListDto
                    {
                        History = new List<RatingHistoryItemDto>(),
                        TotalCount = 0
                    }));
                }

                // Базовый запрос истории
                var baseQuery = _context.RatingHistories
                    .Include(h => h.Tournament)
                    .Where(h => h.PlayerRatingId == rating.Id)
                    .AsQueryable();

                // Фильтры по дате
                if (query.StartDate.HasValue)
                {
                    baseQuery = baseQuery.Where(h => h.ChangeDate >= query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    baseQuery = baseQuery.Where(h => h.ChangeDate <= query.EndDate.Value);
                }

                // Сортировка
                var orderedQuery = query.SortBy switch
                {
                    "date" => query.Descending
                        ? baseQuery.OrderByDescending(h => h.ChangeDate)
                        : baseQuery.OrderBy(h => h.ChangeDate),
                    "change" => query.Descending
                        ? baseQuery.OrderByDescending(h => h.RatingChange)
                        : baseQuery.OrderBy(h => h.RatingChange),
                    "rating" => query.Descending
                        ? baseQuery.OrderByDescending(h => h.NewRating)
                        : baseQuery.OrderBy(h => h.NewRating),
                    _ => baseQuery.OrderByDescending(h => h.ChangeDate)
                };

                // Общее количество
                var totalCount = await orderedQuery.CountAsync();

                // Пагинация
                var history = await orderedQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(h => new RatingHistoryItemDto
                    {
                        Id = h.Id,
                        TournamentId = h.TournamentId,
                        TournamentName = h.Tournament != null ? h.Tournament.Name : "Неизвестный турнир",
                        TournamentDate = h.Tournament != null ? h.Tournament.StartDate : h.ChangeDate,
                        OldRating = h.OldRating,
                        NewRating = h.NewRating,
                        RatingChange = h.RatingChange,
                        ChangeReason = h.ChangeReason,
                        ChangeDate = h.ChangeDate
                    })
                    .ToListAsync();

                var result = new RatingHistoryListDto
                {
                    History = history,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return Ok(ApiResponse<RatingHistoryListDto>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении истории рейтинга игрока {PlayerId}", id);
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        #endregion

        #region Административные методы

        /// <summary>
        /// Обновить рейтинги после турнира (только для админов)
        /// POST: api/ratings/tournament/{id}/update
        /// </summary>
        [HttpPost("tournament/{id}/update")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> UpdateTournamentRatings(int id)
        {
            try
            {
                await _ratingService.UpdateRatingsAfterTournamentAsync(id);
                return Ok(ApiResponse<String>.Success("Рейтинги успешно обновлены"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении рейтингов турнира {TournamentId}", id);
                return StatusCode(500, ApiResponse.Error($"Ошибка: {ex.Message}"));
            }
        }

        /// <summary>
        /// Пересчитать все рейтинги (только для суперадминов)
        /// POST: api/ratings/recalculate-all
        /// </summary>
        [HttpPost("recalculate-all")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RecalculateAllRatings()
        {
            try
            {
                // Запускаем в фоновом режиме
                _ = Task.Run(async () =>
                {
                    await _ratingService.RecalculateAllRatingsAsync();
                });

                return Ok(ApiResponse<String>.Success("Запущен полный перерасчет рейтингов"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при запуске перерасчета всех рейтингов");
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить прогноз матча между двумя игроками
        /// GET: api/ratings/predict
        /// </summary>
        [HttpGet("predict")]
        [AllowAnonymous]
        public async Task<IActionResult> PredictMatch(
            [FromQuery] int player1Id,
            [FromQuery] int player2Id,
            [FromQuery] TournamentType tournamentType = TournamentType.Individual)
        {
            try
            {
                var prediction = await _ratingService.PredictMatchAsync(player1Id, player2Id, tournamentType);
                return Ok(ApiResponse<MatchPrediction>.Success(prediction));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при прогнозе матча {Player1Id} vs {Player2Id}", player1Id, player2Id);
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }
        private static RecalculationProgress _progress = new();

        // GET: api/ratings/recalculation-progress
        [HttpGet("recalculation-progress")]
        [AllowAnonymous]
        public IActionResult GetRecalculationProgress()
        {
            return Ok(ApiResponse<RecalculationProgress>.Success(_progress));
        }


        #endregion

        #region Вспомогательные методы


        /// <summary>
        /// Пересчитать все рейтинги
        /// </summary>
        public async Task RecalculateAllRatingsAsync()
        {
            _logger.LogInformation("Начало перерасчета всех рейтингов");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Получаем все турниры по дате
                var tournaments = await _context.Tournaments
                    .Where(t => t.StartDate.HasValue)
                    .OrderBy(t => t.StartDate)
                    .ToListAsync();

                if (!tournaments.Any())
                {
                    _logger.LogWarning("Нет турниров для перерасчета");
                    return;
                }

                // Инициализируем прогресс
                _progress = new RecalculationProgress
                {
                    StartedAt = DateTime.UtcNow,
                    Completed = false,
                    Current = 0,
                    Total = tournaments.Count,
                    Operation = "Начало перерасчета"
                };

                _logger.LogInformation("Найдено {Count} турниров для перерасчета", tournaments.Count);

                // Очищаем текущие рейтинги и историю
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM RatingHistories");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM PlayerRatings");

                // Сбрасываем флаги обновления у турниров
                await _context.Tournaments
                    .ForEachAsync(t =>
                    {
                        t.RatingsUpdated = false;
                        t.RatingsUpdatedDate = null;
                    });
                await _context.SaveChangesAsync();

                // Пересчитываем рейтинги для каждого турнира по порядку
                int processedCount = 0;
                foreach (var tournament in tournaments)
                {
                    try
                    {
                        // Обновляем прогресс
                        _progress.Current = processedCount;
                        _progress.Operation = $"Обработка турнира: {tournament.Name}";

                        _logger.LogDebug("Обработка турнира {TournamentId}: {TournamentName}",
                            tournament.Id, tournament.Name);

                        // Вызываем метод обновления рейтингов для текущего турнира
                        await _ratingService.UpdateRatingsAfterTournamentAsync(tournament.Id);

                        processedCount++;
                        _progress.Current = processedCount;

                        if (processedCount % 10 == 0 || processedCount == tournaments.Count)
                        {
                            _logger.LogInformation("Обработано {ProcessedCount}/{TotalCount} турниров",
                                processedCount, tournaments.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при обработке турнира {TournamentId}: {TournamentName}",
                            tournament.Id, tournament.Name);

                        // Продолжаем обработку других турниров, но отмечаем прогресс
                        processedCount++;
                        _progress.Current = processedCount;
                        _progress.Operation = $"Ошибка в турнире: {tournament.Name}. Продолжение...";

                        // Непрерывное выполнение - пропускаем проблемный турнир
                        continue;
                    }
                }

                await transaction.CommitAsync();

                // Финальное обновление прогресса
                _progress.Completed = true;
                _progress.Current = _progress.Total;
                _progress.Operation = "Перерасчет завершен";
                _progress.FinishedAt = DateTime.UtcNow;

                _logger.LogInformation("Перерасчет всех рейтингов завершен. Обработано {ProcessedCount} из {TotalCount} турниров",
                    processedCount, tournaments.Count);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Обновляем прогресс с ошибкой
                _progress.Completed = true;
                _progress.Operation = $"Ошибка: {ex.Message}";
                _progress.HasError = true;

                _logger.LogError(ex, "Критическая ошибка при перерасчете всех рейтингов");
                throw;
            }
        }
        private async Task<int> GetPlayerGlobalPlaceAsync(int playerId)
        {
            var rating = await _context.PlayerRatings
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (rating == null) return 0;

            var higherRatingsCount = await _context.PlayerRatings
                .CountAsync(r => r.Rating > rating.Rating);

            return higherRatingsCount + 1;
        }

        private async Task<int> GetRatingChangeAsync(int playerId, DateTime sinceDate)
        {
            var rating = await _context.PlayerRatings
                .Include(r => r.History)
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (rating == null || rating.History == null) return 0;

            var lastChange = rating.History
                .Where(h => h.ChangeDate <= sinceDate)
                .OrderByDescending(h => h.ChangeDate)
                .FirstOrDefault();

            return lastChange != null ? rating.Rating - lastChange.NewRating : 0;
        }

        private async Task<int> CalculateCurrentStreakAsync(int playerId)
        {
            var rating = await _context.PlayerRatings
                .Include(r => r.History)
                .FirstOrDefaultAsync(r => r.PlayerId == playerId);

            if (rating?.History == null || !rating.History.Any()) return 0;

            var sortedHistory = rating.History
                .OrderByDescending(h => h.ChangeDate)
                .ToList();

            int streak = 0;
            for (int i = 0; i < sortedHistory.Count; i++)
            {
                if (sortedHistory[i].RatingChange > 0)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private double CalculateConsistency(PlayerRating rating)
        {
            if (rating.TournamentCount < 2) return 100;

            // Рассчитываем стандартное отклонение мест
            // Чем меньше отклонение, тем выше консистентность
            double consistency = 100 - (rating.AveragePlace * 2);
            return Math.Max(0, Math.Min(100, consistency));
        }

        private async Task<Dictionary<string, int>> GetRatingDistributionAsync()
        {
            return new Dictionary<string, int>
            {
                ["<1300"] = await _context.PlayerRatings.CountAsync(r => r.Rating < 1300),
                ["1300-1499"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 1300 && r.Rating < 1500),
                ["1500-1699"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 1500 && r.Rating < 1700),
                ["1700-1899"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 1700 && r.Rating < 1900),
                ["1900-2099"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 1900 && r.Rating < 2100),
                ["2100+"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 2100)
            };
        }

        private async Task<Dictionary<string, int>> GetTournamentDistributionAsync()
        {
            return new Dictionary<string, int>
            {
                ["1-5"] = await _context.PlayerRatings.CountAsync(r => r.TournamentCount >= 1 && r.TournamentCount <= 5),
                ["6-10"] = await _context.PlayerRatings.CountAsync(r => r.TournamentCount >= 6 && r.TournamentCount <= 10),
                ["11-20"] = await _context.PlayerRatings.CountAsync(r => r.TournamentCount >= 11 && r.TournamentCount <= 20),
                ["21-50"] = await _context.PlayerRatings.CountAsync(r => r.TournamentCount >= 21 && r.TournamentCount <= 50),
                ["50+"] = await _context.PlayerRatings.CountAsync(r => r.TournamentCount > 50)
            };
        }

        private async Task<List<RegionStatisticDto>> GetTopRegionsAsync(int count)
        {
            return await _context.PlayerRatings
                .Include(r => r.Player)
                    .ThenInclude(p => p.District)
                .Where(r => r.Player.District != null && r.TournamentCount >= 3)
                .GroupBy(r => r.Player.District.Name)
                .Select(g => new RegionStatisticDto
                {
                    Name = g.Key,
                    PlayerCount = g.Count(),
                    AverageRating = (int)g.Average(r => r.Rating),
                    HighestRating = g.Max(r => r.Rating),
                    TotalTournaments = g.Sum(r => r.TournamentCount)
                })
                .OrderByDescending(r => r.AverageRating)
                .Take(count)
                .ToListAsync();
        }

        private async Task<GenderStatisticsDto> GetGenderStatisticsAsync()
        {
            var maleStats = await _context.PlayerRatings
                .Include(r => r.Player)
                .Where(r => r.Player.Gender == Gender.Male && r.TournamentCount >= 3)
                .Select(r => new { r.Rating, r.TournamentCount })
                .ToListAsync();

            var femaleStats = await _context.PlayerRatings
                .Include(r => r.Player)
                .Where(r => r.Player.Gender == Gender.Female && r.TournamentCount >= 3)
                .Select(r => new { r.Rating, r.TournamentCount })
                .ToListAsync();

            return new GenderStatisticsDto
            {
                MaleCount = maleStats.Count,
                MaleAverageRating = maleStats.Any() ? (int)maleStats.Average(r => r.Rating) : 0,
                FemaleCount = femaleStats.Count,
                FemaleAverageRating = femaleStats.Any() ? (int)femaleStats.Average(r => r.Rating) : 0,
                TotalCount = maleStats.Count + femaleStats.Count
            };
        }

        private async Task<double> CalculateMedianRatingAsync()
        {
            var ratings = await _context.PlayerRatings
                .Where(r => r.TournamentCount >= 3)
                .Select(r => (double)r.Rating)
                .OrderBy(r => r)
                .ToListAsync();

            if (!ratings.Any()) return 1500;

            int count = ratings.Count;
            if (count % 2 == 0)
            {
                return (ratings[count / 2 - 1] + ratings[count / 2]) / 2;
            }
            else
            {
                return ratings[count / 2];
            }
        }

        #endregion
    }

    #region DTO классы
    public class RecalculationProgress
    {
        public bool Completed { get; set; }
        public bool HasError { get; set; }
        public int Current { get; set; }
        public int Total { get; set; }
        public string Operation { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public TimeSpan? Duration => FinishedAt.HasValue ? FinishedAt.Value - StartedAt : null;

        public double Percentage => Total > 0 ? (double)Current / Total * 100 : 0;
    }
    public class RankingsQueryDto
    {
        [Range(1, 1000)]
        public int Page { get; set; } = 1;

        [Range(1, 500)]
        public int PageSize { get; set; } = 25;

        public string? Search { get; set; }
        public string? Region { get; set; }
        public Gender? Gender { get; set; }
        public bool OnlyActive { get; set; } = true;
        public int? MinTournaments { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public string SortBy { get; set; } = "rating";
        public bool Descending { get; set; } = true;
    }

    public class RatingHistoryQueryDto
    {
        [Range(1, 1000)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "date";
        public bool Descending { get; set; } = true;
    }

    public class GlobalRankingsDto
    {
        public List<PlayerRankingDto> Players { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PlayerRankingDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Region { get; set; }
        public Gender Gender { get; set; }
        public int Place { get; set; }
        public int Rating { get; set; }
        public int PeakRating { get; set; }
        public int TournamentCount { get; set; }
        public decimal AverageScore { get; set; }
        public double AveragePlace { get; set; }
        public double Top3Percentage { get; set; }
        public double Top10Percentage { get; set; }
        public DateTime LastUpdated { get; set; }
        public int TotalGames { get; set; }
        public int TotalPins { get; set; }
        public int? RatingChange { get; set; }
    }

    public class PlayerRatingDetailDto : PlayerRankingDto
    {
        public double Consistency { get; set; }
        public int Streak { get; set; }
        public int RatingChangeLastMonth { get; set; }
        public DateTime? Created { get; set; }
    }

    public class RatingStatisticsDto
    {
        // Основные показатели
        public int TotalPlayers { get; set; }
        public int ActivePlayers { get; set; }
        public int NewPlayersLastMonth { get; set; }

        // Рейтинговые показатели
        public double AverageRating { get; set; }
        public int HighestRating { get; set; }
        public int LowestRating { get; set; }
        public double MedianRating { get; set; }

        // Распределения
        public Dictionary<string, int> RatingDistribution { get; set; } = new();
        public Dictionary<string, int> TournamentDistribution { get; set; } = new();

        // Регионы
        public List<RegionStatisticDto> TopRegions { get; set; } = new();

        // Статистика по полу
        public GenderStatisticsDto GenderStatistics { get; set; } = new();

        // Турниры
        public int TournamentsProcessed { get; set; }
        public double AverageTournamentsPerPlayer { get; set; }

        // Даты
        public DateTime LastUpdated { get; set; }
        public DateTime OldestRatingDate { get; set; }
    }

    public class RegionStatisticDto
    {
        public string Name { get; set; } = string.Empty;
        public int PlayerCount { get; set; }
        public int AverageRating { get; set; }
        public int HighestRating { get; set; }
        public int TotalTournaments { get; set; }
    }

    public class GenderStatisticsDto
    {
        public int MaleCount { get; set; }
        public int MaleAverageRating { get; set; }
        public int FemaleCount { get; set; }
        public int FemaleAverageRating { get; set; }
        public int TotalCount { get; set; }
    }

    public class RatingHistoryListDto
    {
        public List<RatingHistoryItemDto> History { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class RatingHistoryItemDto
    {
        public long Id { get; set; }
        public long TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public DateTime? TournamentDate { get; set; }
        public int OldRating { get; set; }
        public int NewRating { get; set; }
        public int RatingChange { get; set; }
        public string ChangeReason { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
    }
    
    #endregion
}