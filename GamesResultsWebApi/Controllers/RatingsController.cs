using GamesResults;
using GamesResults.Interfaces;
using GamesResults.Models.Bowling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamesResults.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BowlingStatistic.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Требуем авторизацию для всех методов
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly AppDbContext _context;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(
            IRatingService ratingService,
            AppDbContext context,
            ILogger<RatingsController> logger)
        {
            _ratingService = ratingService;
            _context = context;
            _logger = logger;
        }

        #region Основные GET-методы

        /// <summary>
        /// Получить глобальный рейтинг игроков
        /// </summary>
        [HttpGet("/raiting/global")]
        [AllowAnonymous] // Разрешаем доступ без авторизации
        [ProducesResponseType(typeof(ApiResponse<List<PlayerRatingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGlobalRankings(
            [FromQuery] int top = 100,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? region = null,
            [FromQuery] Gender? gender = null)
        {
            try
            {
                _logger.LogInformation("Запрос глобального рейтинга: Top={Top}, Region={Region}", top, region);

                var query = _context.PlayerRatings
                    .Include(r => r.Player)
                        .ThenInclude(p => p.District)
                    .Where(r => r.TournamentCount >= 3); // Минимум 3 турнира

                // Фильтр по активности
                if (!includeInactive)
                {
                    // Игроки, участвовавшие в турнирах за последний год
                    var oneYearAgo = DateTime.UtcNow.AddYears(-1);
                    query = query.Where(r => r.LastUpdated >= oneYearAgo);
                }

                // Фильтр по региону
                if (!string.IsNullOrEmpty(region))
                {
                    query = query.Where(r => r.Player.District != null &&
                                           r.Player.District.Name.Contains(region));
                }

                // Фильтр по полу
                if (gender.HasValue)
                {
                    query = query.Where(r => r.Player.Gender == gender.Value);
                }

                var ratings = await query
                    .OrderByDescending(r => r.Rating)
                    .ThenByDescending(r => r.TournamentCount)
                    .ThenBy(r => r.AveragePlace)
                    .Take(Math.Min(top, 500)) // Ограничиваем максимум 500
                    .Select(r => MapToDto(r))
                    .ToListAsync();

                var response = new ApiResponse<List<PlayerRatingDto>>
                {
                    Success = true,
                    Data = ratings,
                    Message = $"Получено {ratings.Count} записей рейтинга"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении глобального рейтинга");
                return StatusCode(500, ApiResponse.Fail("Ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить рейтинг конкретного игрока
        /// </summary>
        [HttpGet("/raiting/player/{playerId}")]
        [ProducesResponseType(typeof(ApiResponse<PlayerRatingDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlayerRating(long playerId)
        {
            try
            {
                var rating = await _context.PlayerRatings
                    .Include(r => r.Player)
                        .ThenInclude(p => p.District)
                    .Include(r => r.History)
                        .ThenInclude(h => h.Tournament)
                    .FirstOrDefaultAsync(r => r.PlayerId == playerId);

                if (rating == null)
                {
                    // Создаем начальный рейтинг, если его нет
                    var player = await _context.Players
                        .Include(p => p.District)
                        .FirstOrDefaultAsync(p => p.Id == playerId);

                    if (player == null)
                        return NotFound(ApiResponse.Fail($"Игрок с ID {playerId} не найден"));

                    rating = await _ratingService.GetPlayerRatingAsync(playerId);
                }

                var dto = MapToDetailDto(rating);

                return Ok(ApiResponse<PlayerRatingDetailDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении рейтинга игрока {PlayerId}", playerId);
                return StatusCode(500, ApiResponse.Fail("Ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить историю рейтинга игрока
        /// </summary>
        [HttpGet("/raiting/player/{playerId}/history")]
        [ProducesResponseType(typeof(ApiResponse<List<RatingHistoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRatingHistory(
        long playerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Начинаем с базового запроса
                var baseQuery = _context.RatingHistories
                    .Include(h => h.Tournament)
                    .Include(h => h.PlayerRating)
                        .ThenInclude(pr => pr.Player)
                    .Where(h => h.PlayerRating.PlayerId == playerId);

                // Применяем фильтры
                if (startDate.HasValue)
                    baseQuery = baseQuery.Where(h => h.ChangeDate >= startDate.Value);

                if (endDate.HasValue)
                    baseQuery = baseQuery.Where(h => h.ChangeDate <= endDate.Value);

                // Только ПОСЛЕ фильтров применяем сортировку
                var query = baseQuery.OrderByDescending(h => h.ChangeDate);

                var totalCount = await query.CountAsync();
                var history = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(h => MapToHistoryDto(h))
                    .ToListAsync();

                var response = new ApiResponse<List<RatingHistoryDto>>
                {
                    Success = true,
                    Data = history,
                    Pagination = new PaginationInfo
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении истории рейтинга игрока {PlayerId}", playerId);
                return StatusCode(500, ApiResponse.Fail("Ошибка сервера"));
            }
        }

        #endregion

        #region Административные методы

        /// <summary>
        /// Обновить рейтинги после турнира (только для админов)
        /// </summary>
        [HttpPost("/raiting/tournament/{tournamentId}/update")]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateTournamentRatings(long tournamentId)
        {
            try
            {
                // Проверяем существование турнира
                var tournament = await _context.Tournaments
                    .FirstOrDefaultAsync(t => t.Id == tournamentId);

                if (tournament == null)
                    return NotFound(ApiResponse.Fail($"Турнир с ID {tournamentId} не найден"));

                // Проверяем, не обновлялись ли рейтинги уже
                if (tournament.RatingsUpdated)
                {
                    return BadRequest(ApiResponse.Fail("Рейтинги для этого турнира уже были обновлены"));
                }

                await _ratingService.UpdateRatingsAfterTournamentAsync(tournamentId);

                return Ok(ApiResponse.Ok("Рейтинги успешно обновлены"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении рейтингов турнира {TournamentId}", tournamentId);
                return StatusCode(500, ApiResponse.Fail($"Ошибка: {ex.Message}"));
            }
        }

        /// <summary>
        /// Пересчитать все рейтинги (только для суперадминов)
        /// </summary>
        [HttpPost("/raiting/recalculate-all")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecalculateAllRatings()
        {
            try
            {
                _logger.LogWarning("Начало полного перерасчета всех рейтингов по запросу пользователя {User}",
                    User.Identity?.Name);

                // Запускаем в фоновом режиме, т.к. операция долгая
                Task.Run(async () =>
                {
                    await _ratingService.RecalculateAllRatingsAsync();
                });

                return Ok(ApiResponse.Ok("Запущен полный перерасчет рейтингов. Операция выполняется в фоновом режиме."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при запуске перерасчета всех рейтингов");
                return StatusCode(500, ApiResponse.Fail("Ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить статистику рейтинговой системы
        /// </summary>
        [HttpGet("/raiting/statistics")]
        [Authorize(Roles = "Admin,Organizer")]
        [ProducesResponseType(typeof(ApiResponse<RatingStatisticsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRatingStatistics()
        {
            try
            {
                var stats = new RatingStatisticsDto
                {
                    TotalPlayers = await _context.PlayerRatings.CountAsync(),
                    ActivePlayers = await _context.PlayerRatings
                        .Where(r => r.LastUpdated >= DateTime.UtcNow.AddMonths(-6))
                        .CountAsync(),
                    AverageRating = await _context.PlayerRatings
                        .Where(r => r.TournamentCount >= 3)
                        .AverageAsync(r => (double?)r.Rating) ?? 1500,
                    HighestRating = await _context.PlayerRatings
                        .MaxAsync(r => (int?)r.Rating) ?? 1500,
                    LowestRating = await _context.PlayerRatings
                        .Where(r => r.TournamentCount >= 3)
                        .MinAsync(r => (int?)r.Rating) ?? 1500,
                    LastUpdated = await _context.PlayerRatings
                        .MaxAsync(r => (DateTime?)r.LastUpdated) ?? DateTime.MinValue,
                    TournamentsProcessed = await _context.Tournaments
                        .CountAsync(t => t.RatingsUpdated)
                };

                // Распределение по рейтинговым диапазонам
                stats.RatingDistribution = new Dictionary<string, int>
                {
                    ["<1300"] = await _context.PlayerRatings.CountAsync(r => r.Rating < 1300),
                    ["1300-1499"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 1300 && r.Rating < 1500),
                    ["1500-1699"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 1500 && r.Rating < 1700),
                    ["1700-1899"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 1700 && r.Rating < 1900),
                    ["1900-2099"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 1900 && r.Rating < 2100),
                    ["2100+"] = await _context.PlayerRatings.CountAsync(r => r.Rating >= 2100)
                };

                return Ok(ApiResponse<RatingStatisticsDto>.Ok(stats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики рейтингов");
                return StatusCode(500, ApiResponse.Fail("Ошибка сервера"));
            }
        }

        #endregion

        #region Вспомогательные методы маппинга

        private PlayerRatingDto MapToDto(PlayerRating rating)
        {
            return new PlayerRatingDto
            {
                PlayerId = rating.PlayerId,
                PlayerName = rating.Player?.Name ?? "Неизвестный игрок",
                Region = rating.Player?.District?.Name ?? "Не указан",
                Rating = rating.Rating,
                PeakRating = rating.PeakRating,
                TournamentCount = rating.TournamentCount,
                AveragePlace = rating.AveragePlace,
                AverageScore = rating.AverageScore,
                RankChange = CalculateRankChange(rating),
                LastTournamentDate = rating.LastUpdated
            };
        }

        private PlayerRatingDetailDto MapToDetailDto(PlayerRating rating)
        {
            var dto = new PlayerRatingDetailDto
            {
                PlayerId = rating.PlayerId,
                PlayerName = rating.Player?.Name ?? "Неизвестный игрок",
                Region = rating.Player?.District?.Name ?? "Не указан",
                Gender = rating.Player?.Gender ?? Gender.Unknown,
                Rating = rating.Rating,
                PeakRating = rating.PeakRating,
                TournamentCount = rating.TournamentCount,
                AveragePlace = rating.AveragePlace,
                AverageScore = rating.AverageScore,
                TotalGames = rating.TotalGames,
                TotalPins = rating.TotalPins,
                Top3Percentage = rating.Top3Percentage,
                Top10Percentage = rating.Top10Percentage,
                LastUpdated = rating.LastUpdated,
                History = rating.History?.Select(MapToHistoryDto).ToList() ?? new List<RatingHistoryDto>()
            };

            // Рассчитываем дополнительные метрики
            dto.Consistency = CalculateConsistency(rating);
            dto.Streak = CalculateCurrentStreak(rating.PlayerId);

            return dto;
        }

        private RatingHistoryDto MapToHistoryDto(RatingHistory history)
        {
            return new RatingHistoryDto
            {
                TournamentId = history.TournamentId,
                TournamentName = history.Tournament?.Name ?? "Неизвестный турнир",
                TournamentDate = history.Tournament?.StartDate ?? history.ChangeDate,
                OldRating = history.OldRating,
                NewRating = history.NewRating,
                RatingChange = history.RatingChange,
                ChangeReason = history.ChangeReason,
                ChangeDate = history.ChangeDate
            };
        }

        private int? CalculateRankChange(PlayerRating rating)
        {
            // Здесь можно реализовать логику расчета изменения позиции в рейтинге
            // за последний период (например, за месяц)
            // Для простоты вернем null
            return null;
        }

        private double CalculateConsistency(PlayerRating rating)
        {
            if (rating.TournamentCount < 2) return 100;

            // Простая метрика консистентности на основе отклонения от среднего места
            // Чем меньше отклонение, тем выше консистентность
            return Math.Max(0, 100 - (rating.AveragePlace * 0.5));
        }

        private int CalculateCurrentStreak(long playerId)
        {
            // Здесь можно реализовать расчет текущей серии турниров с улучшением рейтинга
            // Пока возвращаем 0
            return 0;
        }

        #endregion
    }

    #region DTO классы

    public class PlayerRatingDto
    {
        public long PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int PeakRating { get; set; }
        public int TournamentCount { get; set; }
        public double AveragePlace { get; set; }
        public double AverageScore { get; set; }
        public int? RankChange { get; set; } // Изменение позиции (+1, -2 и т.д.)
        public DateTime LastTournamentDate { get; set; }
    }

    public class PlayerRatingDetailDto : PlayerRatingDto
    {
        public Gender Gender { get; set; }
        public int TotalGames { get; set; }
        public int TotalPins { get; set; }
        public double Top3Percentage { get; set; }
        public double Top10Percentage { get; set; }
        public double Consistency { get; set; } // Показатель стабильности (0-100)
        public int Streak { get; set; } // Текущая серия турниров с ростом рейтинга
        public DateTime LastUpdated { get; set; }
        public List<RatingHistoryDto> History { get; set; } = new();
    }

    public class RatingHistoryDto
    {
        public long TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public DateTime TournamentDate { get; set; }
        public int OldRating { get; set; }
        public int NewRating { get; set; }
        public int RatingChange { get; set; }
        public string ChangeReason { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
    }

    public class RatingStatisticsDto
    {
        public int TotalPlayers { get; set; }
        public int ActivePlayers { get; set; }
        public double AverageRating { get; set; }
        public int HighestRating { get; set; }
        public int LowestRating { get; set; }
        public DateTime LastUpdated { get; set; }
        public int TournamentsProcessed { get; set; }
        public Dictionary<string, int> RatingDistribution { get; set; } = new();
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaginationInfo? Pagination { get; set; }

        // Фабричные методы с понятными именами
        public static ApiResponse Ok(string message = "Успешно") => new()
        {
            Success = true,
            Message = message
        };

        public static ApiResponse Fail(string message) => new()
        {
            Success = false,
            Message = message
        };

        // Часто используемые ответы
        public static ApiResponse NotFound => Fail("Ресурс не найден");
        public static ApiResponse Unauthorized => Fail("Требуется авторизация");
        public static ApiResponse Forbidden => Fail("Доступ запрещен");
        public static ApiResponse ValidationError => Fail("Ошибка валидации");

        // Метод для добавления пагинации
        public ApiResponse AddPagination(PaginationInfo pagination)
        {
            Pagination = pagination;
            return this;
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Успешно") => new()
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}