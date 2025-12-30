using GamesResults;
using GamesResults.Utils;
using GamesResults.Models.Bowling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamesResultsWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RegionsController> _logger;

        public RegionsController(AppDbContext context, ILogger<RegionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получить все регионы (районы)
        /// </summary>
        /// <param name="search">Поиск по названию региона</param>
        /// <param name="page">Номер страницы (начиная с 1)</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 50)</param>
        /// <returns>Список регионов</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegionDto>>> GetRegions(
            [FromQuery] string search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.Districts.AsQueryable();

                // Применяем поиск, если указан
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(d => d.Title.ToLower().Contains(search));
                }

                // Получаем общее количество для пагинации
                var totalCount = await query.CountAsync();

                // Применяем пагинацию
                var items = await query
                    .OrderBy(d => d.Title)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new RegionDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.ModifiedAt,
                        PlayerCount = d.Players.Count
                    })
                    .ToListAsync();

                // Возвращаем результат с метаданными пагинации
                var response = new
                {
                    Items = items,
                    Pagination = new
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
                _logger.LogError(ex, "Ошибка при получении списка регионов");
                return StatusCode(500, new { Title = "Ошибка сервера", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Получить регион по ID
        /// </summary>
        /// <param name="id">ID региона</param>
        /// <returns>Регион</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<RegionDto>> GetRegion(int id)
        {
            try
            {
                var region = await _context.Districts
                    .Include(d => d.Players)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (region == null)
                {
                    return NotFound(new { Title = "Регион не найден", Id = id });
                }

                var regionDto = new RegionDto
                {
                    Id = region.Id,
                    Title = region.Title,
                    CreatedAt = region.CreatedAt,
                    UpdatedAt = region.ModifiedAt,
                    PlayerCount = region.Players.Count
                };

                return Ok(regionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении региона с ID {RegionId}", id);
                return StatusCode(500, new { Title = "Ошибка сервера", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Получить регионы с популярностью (чаще всего встречающиеся в турнирах)
        /// </summary>
        /// <param name="limit">Количество возвращаемых регионов</param>
        /// <returns>Список популярных регионов</returns>
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<PopularRegionDto>>> GetPopularRegions([FromQuery] int limit = 10)
        {
            try
            {
                var popularRegions = await _context.Districts
                    .Select(d => new PopularRegionDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        PlayerCount = d.Players.Count,
                        // Можно добавить другие метрики, например количество участий в турнирах
                    })
                    .OrderByDescending(r => r.PlayerCount)
                    .Take(limit)
                    .ToListAsync();

                return Ok(popularRegions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении популярных регионов");
                return StatusCode(500, new { Title = "Ошибка сервера", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Получить статистику по регионам
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetRegionsStatistics()
        {
            try
            {
                var totalRegions = await _context.Districts.CountAsync();
                var totalPlayers = await _context.Players.CountAsync();
                var regionsWithPlayers = await _context.Districts
                    .Where(d => d.Players.Any())
                    .CountAsync();

                // Получаем топ регионов по количеству игроков
                var topRegionsByPlayerCount = await _context.Districts
                    .Include(d => d.Players)
                        .ThenInclude(p => p.PlayerRating)
                    .Select(d => new
                    {
                        Region = d.Title,
                        RegionId = d.Id,
                        PlayerCount = d.Players.Count,
                        MaleCount = d.Players.Count(p => p.Gender == Gender.Male),
                        FemaleCount = d.Players.Count(p => p.Gender == Gender.Female),
                        // Средний рейтинг из связанной таблицы PlayerRating
                        AverageRating = d.Players
                            .Where(p => p.PlayerRating != null)
                            .Average(p => (double?)p.PlayerRating.Rating),
                        // Средний score из истории турниров
                        AverageScore = d.Players
                            .SelectMany(p => p.IndividualResults)
                            .Where(r => r.AverageScore > 0)
                            .Average(r => (double?)r.AverageScore),
                        // Количество турниров с участием игроков из региона
                        TournamentCount = d.Players
                            .SelectMany(p => p.IndividualResults)
                            .Select(r => r.TournamentId)
                            .Distinct()
                            .Count()
                    })
                    .Where(d => d.PlayerCount > 0)
                    .OrderByDescending(d => d.PlayerCount)
                    .Take(5)
                    .ToListAsync();

                // Статистика по гендерному распределению
                var genderStats = await _context.Players
                    .GroupBy(p => p.Gender)
                    .Select(g => new
                    {
                        Gender = g.Key,
                        Count = g.Count(),
                        Percentage = (double)g.Count() / totalPlayers * 100
                    })
                    .ToListAsync();

                // Регионы с наивысшим средним рейтингом
                var topRegionsByRating = await _context.Districts
                    .Include(d => d.Players)
                        .ThenInclude(p => p.PlayerRating)
                    .Where(d => d.Players.Any(p => p.PlayerRating != null))
                    .Select(d => new
                    {
                        Region = d.Title,
                        RegionId = d.Id,
                        PlayerCount = d.Players.Count,
                        AverageRating = d.Players
                            .Where(p => p.PlayerRating != null)
                            .Average(p => (double?)p.PlayerRating.Rating),
                        TopRating = d.Players
                            .Where(p => p.PlayerRating != null)
                            .Max(p => (int?)p.PlayerRating.Rating)
                    })
                    .Where(d => d.AverageRating.HasValue)
                    .OrderByDescending(d => d.AverageRating)
                    .Take(5)
                    .ToListAsync();

                // Статистика по возрастным группам (если есть дата рождения)
                var ageStats = await _context.Players
                    .Where(p => p.BirthDate.HasValue)
                    .GroupBy(p => new
                    {
                        AgeGroup = GetAgeGroup(p.BirthDate.Value)
                    })
                    .Select(g => new
                    {
                        AgeGroup = g.Key.AgeGroup,
                        Count = g.Count(),
                        AverageAge = g.Average(p => DateTime.Now.Year - p.BirthDate.Value.Year)
                    })
                    .ToListAsync();

                // Регионы с наибольшим количеством турниров
                var topRegionsByTournaments = await _context.Districts
                    .Select(d => new
                    {
                        Region = d.Title,
                        RegionId = d.Id,
                        TournamentCount = d.Players
                            .SelectMany(p => p.IndividualResults)
                            .Select(r => r.TournamentId)
                            .Distinct()
                            .Count(),
                        TotalParticipations = d.Players
                            .SelectMany(p => p.IndividualResults)
                            .Count()
                    })
                    .Where(d => d.TournamentCount > 0)
                    .OrderByDescending(d => d.TournamentCount)
                    .Take(5)
                    .ToListAsync();

                // Общая статистика по рейтингам
                var ratingStats = await _context.PlayerRatings
                    .GroupBy(r => r.RatingCategory)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        MinRating = g.Min(r => r.Rating),
                        MaxRating = g.Max(r => r.Rating),
                        AverageRating = g.Average(r => r.Rating)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Summary = new
                    {
                        TotalRegions = totalRegions,
                        TotalPlayers = totalPlayers,
                        RegionsWithPlayers = regionsWithPlayers,
                        AveragePlayersPerRegion = totalRegions > 0 ? (double)totalPlayers / totalRegions : 0,
                        PercentageWithPlayers = totalRegions > 0 ? (double)regionsWithPlayers / totalRegions * 100 : 0
                    },

                    TopRegionsByPlayerCount = topRegionsByPlayerCount,
                    TopRegionsByRating = topRegionsByRating,
                    TopRegionsByTournaments = topRegionsByTournaments,

                    GenderDistribution = genderStats,
                    AgeDistribution = ageStats,
                    RatingDistribution = ratingStats,

                    // Дополнительная статистика
                    AdditionalStats = new
                    {
                        PlayersWithRating = await _context.PlayerRatings.CountAsync(),
                        PlayersWithBirthDate = await _context.Players.CountAsync(p => p.BirthDate.HasValue),
                        PlayersInTeams = await _context.TeamMembers
                            .Select(tm => tm.PlayerId)
                            .Distinct()
                            .CountAsync(),
                        AvgTournamentsPerPlayer = totalPlayers > 0 ?
                            (double)await _context.IndividualResults
                                .Select(r => r.PlayerId)
                                .Distinct()
                                .CountAsync() / totalPlayers : 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики регионов");
                return StatusCode(500, new { Title = "Ошибка сервера", Detail = ex.Message });
            }
        }

        // Вспомогательный метод для определения возрастной группы
        private string GetAgeGroup(DateTime birthDate)
        {
            var age = DateTime.Now.Year - birthDate.Year;

            if (birthDate.Date > DateTime.Now.AddYears(-age)) age--;

            return age switch
            {
                < 18 => "До 18",
                >= 18 and < 25 => "18-24",
                >= 25 and < 35 => "25-34",
                >= 35 and < 45 => "35-44",
                >= 45 and < 55 => "45-54",
                >= 55 and < 65 => "55-64",
                _ => "65+"
            };
        }

        /// <summary>
        /// Создать новый регион
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RegionDto>> CreateRegion([FromBody] CreateRegionDto createDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDto.Title))
                {
                    return BadRequest(new { Title = "Название региона обязательно" });
                }

                // Проверяем, существует ли уже регион с таким названием
                var existingRegion = await _context.Districts
                    .FirstOrDefaultAsync(d => d.Title.ToLower() == createDto.Title.Trim().ToLower());

                if (existingRegion != null)
                {
                    return Conflict(new
                    {
                        Title = "Регион с таким названием уже существует",
                        Region = existingRegion
                    });
                }

                var region = new District
                {
                    Title = createDto.Title.Trim(),
                    Description = createDto.Description?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Districts.Add(region);
                await _context.SaveChangesAsync();

                var regionDto = new RegionDto
                {
                    Id = region.Id,
                    Title = region.Title,
                    CreatedAt = region.CreatedAt,
                    UpdatedAt = region.ModifiedAt,
                    PlayerCount = 0
                };

                return CreatedAtAction(nameof(GetRegion), new { id = region.Id }, regionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании региона");
                return StatusCode(500, new { Title = "Ошибка сервера", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Обновить регион
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<RegionDto>> UpdateRegion(int id, [FromBody] UpdateRegionDto updateDto)
        {
            try
            {
                var region = await _context.Districts.FindAsync(id);

                if (region == null)
                {
                    return NotFound(new { Title = "Регион не найден", Id = id });
                }

                // Проверяем, не существует ли другой регион с таким же названием
                if (!string.IsNullOrWhiteSpace(updateDto.Title))
                {
                    var title = updateDto.Title.Trim();
                    var existingRegion = await _context.Districts
                        .FirstOrDefaultAsync(d => d.Id != id && d.Title.ToLower() == title.ToLower());

                    if (existingRegion != null)
                    {
                        return Conflict(new
                        {
                            Title = "Регион с таким названием уже существует",
                            Region = existingRegion
                        });
                    }

                    region.Title = title;
                }

                if (updateDto.Description != null)
                {
                    region.Description = updateDto.Description.Trim();
                }

                region.ModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var regionDto = new RegionDto
                {
                    Id = region.Id,
                    Title = region.Title,
                    CreatedAt = region.CreatedAt,
                    UpdatedAt = region.ModifiedAt,
                    PlayerCount = await _context.Players.CountAsync(p => p.DistrictId == id)
                };

                return Ok(regionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении региона с ID {RegionId}", id);
                return StatusCode(500, new { Title = "Ошибка сервера", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Удалить регион
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            try
            {
                var region = await _context.Districts
                    .Include(d => d.Players)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (region == null)
                {
                    return NotFound(new { Title = "Регион не найден", Id = id });
                }

                // Проверяем, есть ли связанные игроки
                if (region.Players.Any())
                {
                    return BadRequest(new
                    {
                        Title = "Невозможно удалить регион",
                        Detail = "Существуют игроки, связанные с этим регионом. Сначала удалите или переместите игроков."
                    });
                }

                _context.Districts.Remove(region);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении региона с ID {RegionId}", id);
                return StatusCode(500, new { Title = "Ошибка сервера", Detail = ex.Message });
            }
        }
    }

    // DTO для регионов
    public class RegionDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int PlayerCount { get; set; }
    }

    public class PopularRegionDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public int PlayerCount { get; set; }
    }

    public class CreateRegionDto
    {
        [Required(ErrorMessage = "Название региона обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название региона должно быть от 2 до 100 символов")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }
    }

    public class UpdateRegionDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название региона должно быть от 2 до 100 символов")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; }
    }
}
