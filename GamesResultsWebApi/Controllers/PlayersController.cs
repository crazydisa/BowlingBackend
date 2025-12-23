using GamesResults;
using GamesResults.Models.Bowling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BowlingStatistic.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlayersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(AppDbContext context, ILogger<PlayersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить турниры игрока
        /// GET: api/players/{id}/tournaments
        /// </summary>
        [HttpGet("{id}/tournaments")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PlayerTournamentsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayerTournaments(
            int id,
            [FromQuery] PlayerTournamentsQueryDto query)
        {
            try
            {
                // Проверяем существование игрока
                var playerExists = await _context.Players.AnyAsync(p => p.Id == id);
                if (!playerExists)
                {
                    return NotFound(ApiResponse.Error($"Игрок с ID {id} не найден"));
                }

                // Базовый запрос индивидуальных результатов
                var baseQuery = _context.IndividualResults
                    .Include(r => r.Tournament)
                        .ThenInclude(t => t.Bowling)
                    .Where(r => r.PlayerId == id)
                    .AsQueryable();

                // Фильтры
                if (query.Year.HasValue)
                {
                    baseQuery = baseQuery.Where(r =>
                        r.Tournament.StartDate.HasValue &&
                        r.Tournament.StartDate.Value.Year == query.Year.Value);
                }

                if (query.TournamentType.HasValue)
                {
                    baseQuery = baseQuery.Where(r =>
                        r.Tournament.TournamentType == query.TournamentType.Value);
                }

                // Сортировка
                var orderedQuery = query.SortBy switch
                {
                    "date" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.Tournament.StartDate)
                        : baseQuery.OrderBy(r => r.Tournament.StartDate),
                    "place" => query.Descending
                        ? baseQuery.OrderBy(r => r.Place)
                        : baseQuery.OrderByDescending(r => r.Place),
                    "score" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.TotalScore)
                        : baseQuery.OrderBy(r => r.TotalScore),
                    _ => baseQuery.OrderByDescending(r => r.Tournament.StartDate)
                };

                // Общее количество
                var totalCount = await orderedQuery.CountAsync();

                // Пагинация
                var results = await orderedQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(r => new PlayerTournamentResultDto
                    {
                        TournamentId = r.TournamentId,
                        TournamentName = r.Tournament.Name,
                        TournamentType = r.Tournament.TournamentType,
                        StartDate = r.Tournament.StartDate,
                        EndDate = r.Tournament.EndDate,
                        BowlingCenter = r.Tournament.Bowling != null ? r.Tournament.Bowling.Name : null,
                        Place = r.Place,
                        TotalScore = r.TotalScore,
                        AverageScore = r.AverageScore,
                        GamesPlayed = r.GamesPlayed,
                        HighGame = r.HighGame,
                        LowGame = r.LowGame,
                        ResultDate = r.ResultDate
                    })
                    .ToListAsync();

                // Статистика по турнирам
                var tournamentStats = await baseQuery
                    .GroupBy(r => 1)
                    .Select(g => new PlayerTournamentStatsDto
                    {
                        TotalTournaments = g.Count(),
                        BestPlace = g.Min(r => r.Place),
                        WorstPlace = g.Max(r => r.Place),
                        AveragePlace = g.Average(r => (double)r.Place),
                        AverageScore = g.Average(r => r.AverageScore),
                        TotalGames = g.Sum(r => r.GamesPlayed),
                        TotalPins = g.Sum(r => (int)r.TotalScore),
                        Victories = g.Count(r => r.Place == 1),
                        Top3Count = g.Count(r => r.Place <= 3),
                        Top10Count = g.Count(r => r.Place <= 10)
                    })
                    .FirstOrDefaultAsync();

                var result = new PlayerTournamentsDto
                {
                    Results = results,
                    Statistics = tournamentStats ?? new PlayerTournamentStatsDto(),
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return Ok(ApiResponse<PlayerTournamentsDto>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении турниров игрока {PlayerId}", id);
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить список игроков (автодополнение)
        /// GET: api/players/search
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchPlayers(
            [FromQuery] string term,
            [FromQuery] int limit = 10)
        {
            try
            {
                var players = await _context.Players
                    .Where(p => p.FullName.Contains(term) || p.Name.Contains(term))
                    .Take(limit)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.FullName,
                        Region = p.District != null ? p.District.Name : null,
                        Gender = p.Gender
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<object>>.Success(players.Cast<object>().ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске игроков");
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить детальную информацию об игроке
        /// GET: api/players/{id}
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlayerDetails(int id)
        {
            try
            {
                var player = await _context.Players
                    .Include(p => p.District)
                    .Include(p => p.PlayerRating)
                    .Include(p => p.IndividualResults)
                        .ThenInclude(r => r.Tournament)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (player == null)
                {
                    return NotFound(ApiResponse.Error($"Игрок с ID {id} не найден"));
                }

                var result = new PlayerDetailsDto
                {
                    Id = player.Id,
                    FullName = player.FullName,
                    Name = player.Name,
                    DateOfBirth = player.BirthDate,
                    Gender = player.Gender,
                    Email = player.Email,
                    Phone = player.Phone,
                    Region = player.District != null ? player.District.Name : null,
                    City = player.City?.Name,
                    Rating = player.PlayerRating?.Rating ?? 1500,
                    TournamentCount = player.PlayerRating?.TournamentCount ?? 0,
                    CreatedAt = player.CreatedAt,
                    LastUpdated = player.PlayerRating?.LastUpdated
                };

                return Ok(ApiResponse<PlayerDetailsDto>.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации об игроке {PlayerId}", id);
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }
    }

    #region DTO классы для PlayersController

    public class PlayerTournamentsQueryDto
    {
        [Range(1, 1000)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        public int? Year { get; set; }
        public TournamentType? TournamentType { get; set; }
        public string SortBy { get; set; } = "date";
        public bool Descending { get; set; } = true;
    }

    public class PlayerTournamentsDto
    {
        public List<PlayerTournamentResultDto> Results { get; set; } = new();
        public PlayerTournamentStatsDto Statistics { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PlayerTournamentResultDto
    {
        public long TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public TournamentType TournamentType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? BowlingCenter { get; set; }
        public int Place { get; set; }
        public decimal TotalScore { get; set; }
        public decimal AverageScore { get; set; }
        public int GamesPlayed { get; set; }
        public int HighGame { get; set; }
        public int LowGame { get; set; }
        public DateTime ResultDate { get; set; }
    }

    public class PlayerTournamentStatsDto
    {
        public int TotalTournaments { get; set; }
        public int BestPlace { get; set; }
        public int WorstPlace { get; set; }
        public double AveragePlace { get; set; }
        public decimal AverageScore { get; set; }
        public int TotalGames { get; set; }
        public int TotalPins { get; set; }
        public int Victories { get; set; }
        public int Top3Count { get; set; }
        public int Top10Count { get; set; }
    }

    public class PlayerDetailsDto
    {
        public long Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public GamesResults.Utils.Gender Gender { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
        public int Rating { get; set; }
        public int TournamentCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    #endregion
}