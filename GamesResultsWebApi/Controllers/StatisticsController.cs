using GamesResults;
using GamesResults.Models.Bowling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BowlingStatistic.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(AppDbContext context, ILogger<StatisticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить топ регионов по среднему рейтингу
        /// GET: api/statistics/top-regions
        /// </summary>
        [HttpGet("top-regions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<TopRegionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTopRegions(
            [FromQuery] int top = 10,
            [FromQuery] int minPlayers = 3)
        {
            try
            {
                var topRegions = await _context.PlayerRatings
                    .Include(r => r.Player)
                        .ThenInclude(p => p.District)
                    .Where(r =>
                        r.Player.District != null &&
                        r.TournamentCount >= 3 &&
                        r.Player.District.Name != null &&
                        r.Player.District.Name != "")
                    .GroupBy(r => new
                    {
                        RegionId = r.Player.District.Id,
                        RegionName = r.Player.District.Name,
                        City = r.Player.City != null ? r.Player.City.Name : null
                    })
                    .Select(g => new
                    {
                        g.Key.RegionId,
                        g.Key.RegionName,
                        g.Key.City,
                        PlayerCount = g.Count(),
                        AverageRating = (int)g.Average(r => r.Rating),
                        HighestRating = g.Max(r => r.Rating),
                        TotalTournaments = g.Sum(r => r.TournamentCount),
                        AverageTournaments = g.Average(r => r.TournamentCount),
                        Top3Percentage = g.Average(r => r.Top3Percentage)
                    })
                    .Where(g => g.PlayerCount >= minPlayers)
                    .OrderByDescending(g => g.AverageRating)
                    .Take(top)
                    .Select(g => new TopRegionDto
                    {
                        RegionId = g.RegionId,
                        RegionName = g.RegionName,
                        City = g.City,
                        PlayerCount = g.PlayerCount,
                        AverageRating = g.AverageRating,
                        HighestRating = g.HighestRating,
                        TotalTournaments = g.TotalTournaments,
                        AverageTournaments = Math.Round(g.AverageTournaments, 1),
                        Top3Percentage = Math.Round(g.Top3Percentage, 1),
                        Strength = CalculateRegionStrength(g.AverageRating, g.PlayerCount, g.Top3Percentage)
                    })
                    .ToListAsync();

                // Добавляем места
                for (int i = 0; i < topRegions.Count; i++)
                {
                    topRegions[i].Place = i + 1;
                }

                return Ok(ApiResponse<List<TopRegionDto>>.Success(topRegions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении топ регионов");
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить распределение рейтингов
        /// GET: api/statistics/rating-distribution
        /// </summary>
        [HttpGet("rating-distribution")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<RatingDistributionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRatingDistribution(
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var baseQuery = _context.PlayerRatings.AsQueryable();

                if (!includeInactive)
                {
                    var oneYearAgo = DateTime.UtcNow.AddYears(-1);
                    baseQuery = baseQuery.Where(r => r.LastUpdated >= oneYearAgo);
                }

                var distribution = new RatingDistributionDto
                {
                    // Основные диапазоны
                    Below1300 = await baseQuery.CountAsync(r => r.Rating < 1300),
                    Between1300_1499 = await baseQuery.CountAsync(r => r.Rating >= 1300 && r.Rating < 1500),
                    Between1500_1699 = await baseQuery.CountAsync(r => r.Rating >= 1500 && r.Rating < 1700),
                    Between1700_1899 = await baseQuery.CountAsync(r => r.Rating >= 1700 && r.Rating < 1900),
                    Between1900_2099 = await baseQuery.CountAsync(r => r.Rating >= 1900 && r.Rating < 2100),
                    Above2100 = await baseQuery.CountAsync(r => r.Rating >= 2100),

                    // Дополнительные статистики
                    NoviceCount = await baseQuery.CountAsync(r => r.TournamentCount < 5),
                    IntermediateCount = await baseQuery.CountAsync(r => r.TournamentCount >= 5 && r.TournamentCount < 20),
                    ExpertCount = await baseQuery.CountAsync(r => r.TournamentCount >= 20),

                    // Средние значения по диапазонам
                    AverageBelow1300 = await baseQuery
                        .Where(r => r.Rating < 1300)
                        .AverageAsync(r => (double?)r.Rating) ?? 0,
                    AverageAbove2100 = await baseQuery
                        .Where(r => r.Rating >= 2100)
                        .AverageAsync(r => (double?)r.Rating) ?? 0
                };

                // Рассчитываем проценты
                var total = distribution.TotalPlayers;
                if (total > 0)
                {
                    distribution.Below1300Percent = Math.Round(distribution.Below1300 * 100.0 / total, 1);
                    distribution.Between1300_1499Percent = Math.Round(distribution.Between1300_1499 * 100.0 / total, 1);
                    distribution.Between1500_1699Percent = Math.Round(distribution.Between1500_1699 * 100.0 / total, 1);
                    distribution.Between1700_1899Percent = Math.Round(distribution.Between1700_1899 * 100.0 / total, 1);
                    distribution.Between1900_2099Percent = Math.Round(distribution.Between1900_2099 * 100.0 / total, 1);
                    distribution.Above2100Percent = Math.Round(distribution.Above2100 * 100.0 / total, 1);
                }

                return Ok(ApiResponse<RatingDistributionDto>.Success(distribution));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении распределения рейтингов");
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить общую статистику системы
        /// GET: api/statistics/system
        /// </summary>
        [HttpGet("system")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemStatistics()
        {
            try
            {
                var statistics = new SystemStatisticsDto
                {
                    // Игроки
                    TotalPlayers = await _context.Players.CountAsync(),
                    PlayersWithRating = await _context.PlayerRatings.CountAsync(),
                    ActivePlayers = await _context.PlayerRatings
                        .CountAsync(r => r.LastUpdated >= DateTime.UtcNow.AddMonths(-6)),

                    // Турниры
                    TotalTournaments = await _context.Tournaments.CountAsync(),
                    TournamentsThisYear = await _context.Tournaments
                        .CountAsync(t => t.StartDate.HasValue &&
                                       t.StartDate.Value.Year == DateTime.UtcNow.Year),
                    TournamentsWithRatings = await _context.Tournaments
                        .CountAsync(t => t.RatingsUpdated),

                    // Результаты
                    TotalIndividualResults = await _context.IndividualResults.CountAsync(),
                    TotalTeamResults = await _context.TeamResults.CountAsync(),
                    AverageResultsPerTournament = await _context.Tournaments
                        .Select(t => t.Results.Count)
                        .AverageAsync(),

                    // Команды
                    TotalTeams = await _context.Teams.CountAsync(),
                    AverageTeamSize = await _context.Teams
                        .Select(t => t.Members.Count)
                        .AverageAsync(),

                    // Рейтинги
                    TotalRatingChanges = await _context.RatingHistories.CountAsync(),
                    AverageRatingChange = await _context.RatingHistories
                        .AverageAsync(h => (double?)Math.Abs(h.RatingChange)) ?? 0,

                    // Производительность
                    LastDataUpdate = await _context.PlayerRatings
                        .MaxAsync(r => (DateTime?)r.LastUpdated) ?? DateTime.MinValue,
                    DatabaseSize = await GetDatabaseSizeAsync()
                };

                // Тенденции
                statistics.MonthlyGrowth = await CalculateMonthlyGrowthAsync();
                statistics.PopularTournamentTypes = await GetPopularTournamentTypesAsync();
                statistics.BusiestMonths = await GetBusiestMonthsAsync();

                return Ok(ApiResponse<SystemStatisticsDto>.Success(statistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении системной статистики");
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        #region Вспомогательные методы

        private double CalculateRegionStrength(double averageRating, int playerCount, double top3Percentage)
        {
            // Формула силы региона:
            // 40% - средний рейтинг (нормализованный до 2000)
            // 30% - количество игроков (нормализованное)
            // 30% - процент попадания в топ-3

            double ratingScore = (averageRating / 2000.0) * 0.4;
            double playerScore = (Math.Min(playerCount, 50) / 50.0) * 0.3;
            double top3Score = (top3Percentage / 100.0) * 0.3;

            return Math.Round((ratingScore + playerScore + top3Score) * 100, 1);
        }

        private async Task<string> GetDatabaseSizeAsync()
        {
            // Для PostgreSQL
            var size = await _context.Database
                .SqlQueryRaw<long>("SELECT pg_database_size(current_database())")
                .FirstOrDefaultAsync();

            if (size < 1024) return $"{size} B";
            if (size < 1024 * 1024) return $"{size / 1024.0:F1} KB";
            if (size < 1024 * 1024 * 1024) return $"{size / (1024.0 * 1024.0):F1} MB";
            return $"{size / (1024.0 * 1024.0 * 1024.0):F1} GB";
        }

        private async Task<List<MonthlyGrowthDto>> CalculateMonthlyGrowthAsync()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

            var monthlyData = await _context.PlayerRatings
                .Where(r => r.CreatedAt >= sixMonthsAgo)
                .GroupBy(r => new
                {
                    Year = r.CreatedAt.Value.Year,
                    Month = r.CreatedAt.Value.Month
                })
                .Select(g => new MonthlyGrowthDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    NewPlayers = g.Count(),
                    MonthName = $"{g.Key.Year}-{g.Key.Month:00}"
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .Take(6)
                .ToListAsync();

            return monthlyData;
        }

        private async Task<List<TournamentTypeStatDto>> GetPopularTournamentTypesAsync()
        {
            return await _context.Tournaments
                .Where(t => t.TournamentType != TournamentType.Unknown)
                .GroupBy(t => t.TournamentType)
                .Select(g => new TournamentTypeStatDto
                {
                    TournamentType = g.Key,
                    Count = g.Count(),
                    Percentage = Math.Round(g.Count() * 100.0 / _context.Tournaments.Count(), 1)
                })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<BusyMonthDto>> GetBusiestMonthsAsync()
        {
            return await _context.Tournaments
                .Where(t => t.StartDate.HasValue)
                .GroupBy(t => t.StartDate.Value.Month)
                .Select(g => new BusyMonthDto
                {
                    Month = g.Key,
                    MonthName = GetMonthName(g.Key),
                    TournamentCount = g.Count(),
                    AverageParticipants = (int)g.Average(t => t.Results.Count)
                })
                .OrderByDescending(g => g.TournamentCount)
                .Take(6)
                .ToListAsync();
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Январь",
                2 => "Февраль",
                3 => "Март",
                4 => "Апрель",
                5 => "Май",
                6 => "Июнь",
                7 => "Июль",
                8 => "Август",
                9 => "Сентябрь",
                10 => "Октябрь",
                11 => "Ноябрь",
                12 => "Декабрь",
                _ => "Неизвестно"
            };
        }

        #endregion
    }

    #region DTO классы для StatisticsController

    public class TopRegionDto
    {
        public int Place { get; set; }
        public long RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public string? City { get; set; }
        public int PlayerCount { get; set; }
        public int AverageRating { get; set; }
        public int HighestRating { get; set; }
        public int TotalTournaments { get; set; }
        public double AverageTournaments { get; set; }
        public double Top3Percentage { get; set; }
        public double Strength { get; set; }
    }

    public class RatingDistributionDto
    {
        // Количество игроков по диапазонам
        public int Below1300 { get; set; }
        public int Between1300_1499 { get; set; }
        public int Between1500_1699 { get; set; }
        public int Between1700_1899 { get; set; }
        public int Between1900_2099 { get; set; }
        public int Above2100 { get; set; }

        // Проценты
        public double Below1300Percent { get; set; }
        public double Between1300_1499Percent { get; set; }
        public double Between1500_1699Percent { get; set; }
        public double Between1700_1899Percent { get; set; }
        public double Between1900_2099Percent { get; set; }
        public double Above2100Percent { get; set; }

        // Дополнительная статистика
        public int NoviceCount { get; set; }
        public int IntermediateCount { get; set; }
        public int ExpertCount { get; set; }
        public double AverageBelow1300 { get; set; }
        public double AverageAbove2100 { get; set; }

        // Вычисляемые свойства
        [JsonIgnore]
        public int TotalPlayers => Below1300 + Between1300_1499 + Between1500_1699 +
                                  Between1700_1899 + Between1900_2099 + Above2100;
    }

    public class SystemStatisticsDto
    {
        // Игроки
        public int TotalPlayers { get; set; }
        public int PlayersWithRating { get; set; }
        public int ActivePlayers { get; set; }

        // Турниры
        public int TotalTournaments { get; set; }
        public int TournamentsThisYear { get; set; }
        public int TournamentsWithRatings { get; set; }

        // Результаты
        public int TotalIndividualResults { get; set; }
        public int TotalTeamResults { get; set; }
        public double AverageResultsPerTournament { get; set; }

        // Команды
        public int TotalTeams { get; set; }
        public double AverageTeamSize { get; set; }

        // Рейтинги
        public int TotalRatingChanges { get; set; }
        public double AverageRatingChange { get; set; }

        // Производительность
        public DateTime LastDataUpdate { get; set; }
        public string DatabaseSize { get; set; } = string.Empty;

        // Тенденции
        public List<MonthlyGrowthDto> MonthlyGrowth { get; set; } = new();
        public List<TournamentTypeStatDto> PopularTournamentTypes { get; set; } = new();
        public List<BusyMonthDto> BusiestMonths { get; set; } = new();
    }

    public class MonthlyGrowthDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int NewPlayers { get; set; }
    }

    public class TournamentTypeStatDto
    {
        public TournamentType TournamentType { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class BusyMonthDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TournamentCount { get; set; }
        public int AverageParticipants { get; set; }
    }

    #endregion
}