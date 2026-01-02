using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GamesResults;
using GamesResults.Utils;
using GamesResults.Models.Bowling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BowlingStatistic.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TournamentResultsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TournamentResultsController> _logger;

        public TournamentResultsController(
            AppDbContext context,
            ILogger<TournamentResultsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Основные методы

        /// <summary>
        /// Получить все результаты турнира (индивидуальные и командные)
        /// GET: api/tournamentresults/tournament/{tournamentId}
        /// </summary>
        [HttpGet("tournament/{tournamentId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TournamentResultsDto>>> GetTournamentResults(
            long tournamentId,
            [FromQuery] TournamentResultsQueryDto query)
        {
            try
            {
                _logger.LogInformation("Запрос результатов турнира {TournamentId}", tournamentId);

                // Проверяем существование турнира
                var tournamentExists = await _context.Tournaments
                    .AnyAsync(t => t.Id == tournamentId);

                if (!tournamentExists)
                {
                    return NotFound(ApiResponse.Error("Турнир не найден"));
                }

                // Базовые запросы
                var individualQuery = _context.IndividualResults
                    .Include(r => r.Player)
                        .ThenInclude(p => p.City)
                    .Include(r => r.Tournament)
                    .Where(r => r.TournamentId == tournamentId)
                    .AsQueryable();

                var teamQuery = _context.TeamResults
                    .Include(r => r.Team)
                    .Include(r => r.Tournament)
                    .Where(r => r.TournamentId == tournamentId)
                    .AsQueryable();

                // Применяем фильтры
                if (!string.IsNullOrEmpty(query.Search))
                {
                    individualQuery = individualQuery.Where(r =>
                        r.Player.FullName.Contains(query.Search) ||
                        r.Player.Name.Contains(query.Search));
                        //r.Player.FirstName.Contains(query.Search) ||
                        //r.Player.LastName.Contains(query.Search));

                    teamQuery = teamQuery.Where(r =>
                        r.Team.Name.Contains(query.Search));
                }

                if (query.MinPlace.HasValue)
                {
                    individualQuery = individualQuery.Where(r => r.Place >= query.MinPlace.Value);
                    teamQuery = teamQuery.Where(r => r.Place >= query.MinPlace.Value);
                }

                if (query.MaxPlace.HasValue)
                {
                    individualQuery = individualQuery.Where(r => r.Place <= query.MaxPlace.Value);
                    teamQuery = teamQuery.Where(r => r.Place <= query.MaxPlace.Value);
                }

                if (query.MinScore.HasValue)
                {
                    individualQuery = individualQuery.Where(r => r.TotalScore >= query.MinScore.Value);
                    teamQuery = teamQuery.Where(r => r.TotalScore >= query.MinScore.Value);
                }

                // Получаем данные
                var individualResults = await individualQuery.ToListAsync();
                var teamResults = await teamQuery.ToListAsync();

                // Преобразуем в DTO
                var resultDto = new TournamentResultsDto
                {
                    TournamentId = tournamentId,
                    IndividualResults = individualResults.Select(ConvertToIndividualDto).ToList(),
                    TeamResults = teamResults.Select(ConvertToTeamDto).ToList(),
                    Statistics = await CalculateTournamentStatistics(tournamentId, individualResults, teamResults)
                };

                // Применяем пагинацию
                if (query.ResultsType == ResultsType.Individual)
                {
                    var totalCount = resultDto.IndividualResults.Count;
                    resultDto.IndividualResults = resultDto.IndividualResults
                        .Skip((query.Page - 1) * query.PageSize)
                        .Take(query.PageSize)
                        .ToList();
                    resultDto.TotalCount = totalCount;
                }
                else if (query.ResultsType == ResultsType.Team)
                {
                    var totalCount = resultDto.TeamResults.Count;
                    resultDto.TeamResults = resultDto.TeamResults
                        .Skip((query.Page - 1) * query.PageSize)
                        .Take(query.PageSize)
                        .ToList();
                    resultDto.TotalCount = totalCount;
                }
                else
                {
                    resultDto.TotalCount = resultDto.IndividualResults.Count + resultDto.TeamResults.Count;
                }

                resultDto.Page = query.Page;
                resultDto.PageSize = query.PageSize;
                resultDto.TotalPages = (int)Math.Ceiling(resultDto.TotalCount / (double)query.PageSize);

                return Ok(ApiResponse<TournamentResultsDto>.Success(resultDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении результатов турнира {TournamentId}", tournamentId);
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить только индивидуальные результаты
        /// GET: api/tournamentresults/tournament/{tournamentId}/individual
        /// </summary>
        [HttpGet("tournament/{tournamentId}/individual")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IndividualResultsDto>>> GetIndividualResults(
            long tournamentId,
            [FromQuery] ResultsQueryDto query)
        {
            try
            {
                // Базовый запрос
                var baseQuery = _context.IndividualResults
                    .Include(r => r.Player)
                        .ThenInclude(p => p.City)
                    .Include(r => r.Tournament)
                    .Where(r => r.TournamentId == tournamentId)
                    .AsQueryable();

                // Применяем фильтры
                if (!string.IsNullOrEmpty(query.Search))
                {
                    baseQuery = baseQuery.Where(r =>
                        r.Player.FullName.Contains(query.Search) ||
                       r.Player.Name.Contains(query.Search));
                    //r.Player.FirstName.Contains(query.Search) ||
                    //r.Player.LastName.Contains(query.Search));
                }

                if (query.MinPlace.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.Place >= query.MinPlace.Value);
                }

                if (query.MaxPlace.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.Place <= query.MaxPlace.Value);
                }

                if (query.MinScore.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.TotalScore >= query.MinScore.Value);
                }

                // Сортировка
                var orderedQuery = query.SortBy switch
                {
                    "place" => query.Descending
                        ? baseQuery.OrderBy(r => r.Place)
                        : baseQuery.OrderByDescending(r => r.Place),
                    "score" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.TotalScore)
                        : baseQuery.OrderBy(r => r.TotalScore),
                    "average" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.AverageScore)
                        : baseQuery.OrderBy(r => r.AverageScore),
                    "name" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.Player.FullName)
                        : baseQuery.OrderBy(r => r.Player.FullName),
                    _ => baseQuery.OrderBy(r => r.Place)
                };

                // Получаем общее количество
                var totalCount = await orderedQuery.CountAsync();

                // Получаем данные с пагинацией
                var results = await orderedQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(r => new IndividualResultDto
                    {
                        Id = r.Id,
                        Place = r.Place,
                        PlayerId = r.PlayerId,
                        PlayerName = string.IsNullOrEmpty(r.Player.FullName)? r.Player.Name: r.Player.FullName,
                        PlayerRegion = r.Player.City != null ? r.Player.City.Name : null,
                        PlayerGender = r.Player.Gender,
                        TotalScore = r.TotalScore,
                        AverageScore = r.AverageScore,
                        GamesPlayed = r.GamesPlayed,
                        Game1 = r.Game1 == 0 ? r.GameScores[0]: r.Game1,
                        Game2 = r.Game2 == 0 ? r.GameScores[1] : r.Game2,
                        Game3 = r.Game3 == 0 ? r.GameScores[2] : r.Game3,
                        Game4 = r.Game4 == 0 ? r.GameScores[3] : r.Game4,
                        Game5 = r.Game5 == 0 ? r.GameScores[4] : r.Game5,
                        Game6 = r.Game6 == 0 ? r.GameScores[5] : r.Game6,
                        HighGame = r.HighGame,
                        LowGame = r.LowGame,
                        StrikeCount = r.StrikeCount,
                        SpareCount = r.SpareCount,
                        Notes = r.Notes,
                        ResultDate = r.ResultDate
                    })
                    .ToListAsync();

                var resultDto = new IndividualResultsDto
                {
                    TournamentId = tournamentId,
                    Results = results,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return Ok(ApiResponse<IndividualResultsDto>.Success(resultDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении индивидуальных результатов турнира {TournamentId}", tournamentId);
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        /// <summary>
        /// Получить только командные результаты
        /// GET: api/tournamentresults/tournament/{tournamentId}/team
        /// </summary>
        [HttpGet("tournament/{tournamentId}/team")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<TeamResultsDto>>> GetTeamResults(
            long tournamentId,
            [FromQuery] ResultsQueryDto query)
        {
            try
            {
                // Базовый запрос
                var baseQuery = _context.TeamResults
                    .Include(r => r.Team)
                    .Include(r => r.Tournament)
                    .Where(r => r.TournamentId == tournamentId)
                    .AsQueryable();

                // Применяем фильтры
                if (!string.IsNullOrEmpty(query.Search))
                {
                    baseQuery = baseQuery.Where(r => r.Team.Name.Contains(query.Search));
                }

                if (query.MinPlace.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.Place >= query.MinPlace.Value);
                }

                if (query.MaxPlace.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.Place <= query.MaxPlace.Value);
                }

                if (query.MinScore.HasValue)
                {
                    baseQuery = baseQuery.Where(r => r.TotalScore >= query.MinScore.Value);
                }

                // Сортировка
                var orderedQuery = query.SortBy switch
                {
                    "place" => query.Descending
                        ? baseQuery.OrderBy(r => r.Place)
                        : baseQuery.OrderByDescending(r => r.Place),
                    "score" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.TotalScore)
                        : baseQuery.OrderBy(r => r.TotalScore),
                    "name" => query.Descending
                        ? baseQuery.OrderByDescending(r => r.Team.Name)
                        : baseQuery.OrderBy(r => r.Team.Name),
                    _ => baseQuery.OrderBy(r => r.Place)
                };

                // Получаем общее количество
                var totalCount = await orderedQuery.CountAsync();

                // Получаем данные с пагинацией
                var results = await orderedQuery
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(r => new TeamResultDto
                    {
                        Id = r.Id,
                        Place = r.Place,
                        TeamId = r.TeamId,
                        TeamName = r.Team.Name,
                        TeamAbbreviation = r.Team.Abbreviation,
                        TeamRegion = "",//r.Team.City != null ? r.Team.City.Name : null,
                        TotalScore = r.TotalScore,
                        AverageScore = r.AverageScore,
                        GamesPlayed = r.GamesPlayed,
                        TeamSize = r.MemberScores.Count,
                        AveragePerMember = r.TeamSize > 0 ? r.TotalScore / r.TeamSize : 0,
                        Game1 = r.Game1,
                        Game2 = r.Game2,
                        Game3 = r.Game3,
                        Game4 = r.Game4,
                        Game5 = r.Game5,
                        Game6 = r.Game6,
                        Notes = r.Notes,
                        ResultDate = r.ResultDate,
                        MemberScores = r.MemberScores
                    })
                    .ToListAsync();

                // Для каждого результата загружаем детали участников команды
                foreach (var result in results)
                {
                    result.TeamMembers = await GetTeamMembersDetails(result.TeamId);
                }

                var resultDto = new TeamResultsDto
                {
                    TournamentId = tournamentId,
                    Results = results,
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
                };

                return Ok(ApiResponse<TeamResultsDto>.Success(resultDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении командных результатов турнира {TournamentId}", tournamentId);
                return StatusCode(500, ApiResponse.Error("Внутренняя ошибка сервера"));
            }
        }

        #endregion

        #region Вспомогательные методы

        private IndividualResultDto ConvertToIndividualDto(IndividualResult result)
        {
            return new IndividualResultDto
            {
                Id = result.Id,
                Place = result.Place,
                PlayerId = result.PlayerId,
                PlayerName = result.Player?.FullName ?? "Неизвестный игрок",
                PlayerRegion = result.Player?.City?.Name,
                PlayerGender = result.Player?.Gender ?? Gender.Unknown,
                TotalScore = result.TotalScore,
                AverageScore = result.AverageScore,
                GamesPlayed = result.GamesPlayed,
                Game1 = result.Game1,
                Game2 = result.Game2,
                Game3 = result.Game3,
                Game4 = result.Game4,
                Game5 = result.Game5,
                Game6 = result.Game6,
                HighGame = result.HighGame,
                LowGame = result.LowGame,
                StrikeCount = result.StrikeCount,
                SpareCount = result.SpareCount,
                Notes = result.Notes,
                ResultDate = result.ResultDate,
                Consistency = result.Consistency
            };
        }

        private TeamResultDto ConvertToTeamDto(TeamResult result)
        {
            return new TeamResultDto
            {
                Id = result.Id,
                Place = result.Place,
                TeamId = result.TeamId,
                TeamName = result.Team?.Name ?? "Неизвестная команда",
                TeamAbbreviation = result.Team?.Abbreviation,
                TeamRegion = "",//result.Team?.City?.Name,
                TotalScore = result.TotalScore,
                AverageScore = result.AverageScore,
                GamesPlayed = result.GamesPlayed,
                TeamSize = result.TeamSize,
                AveragePerMember = result.AveragePerMember,
                Game1 = result.Game1,
                Game2 = result.Game2,
                Game3 = result.Game3,
                Game4 = result.Game4,
                Game5 = result.Game5,
                Game6 = result.Game6,
                Notes = result.Notes,
                ResultDate = result.ResultDate,
                MemberScores = result.MemberScores
            };
        }

        private async Task<List<TeamMemberDto>> GetTeamMembersDetails(long teamId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.Player)
                .Include(tm => tm.Team)
                .Where(tm => tm.TeamId == teamId && tm.IsActive)
                .Select(tm => new TeamMemberDto
                {
                    PlayerId = tm.PlayerId,
                    PlayerName = tm.Player.FullName,
                    Role = tm.Role,
                    JoinedDate = tm.JoinedDate,
                    JerseyNumber = tm.OrderNumber,
                    AverageScore = tm.AverageInTeam
                })
                .ToListAsync();
        }

        private async Task<TournamentStatisticsDto> CalculateTournamentStatistics(
            long tournamentId,
            List<IndividualResult> individualResults,
            List<TeamResult> teamResults)
        {
            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
                return new TournamentStatisticsDto();

            var statistics = new TournamentStatisticsDto
            {
                TournamentType = tournament.TournamentType,
                TotalParticipants = individualResults.Count + teamResults.Sum(t => t.TeamSize),
                IndividualParticipants = individualResults.Count,
                TeamParticipants = teamResults.Count,
                TeamsCount = teamResults.Count,
                TotalTeamsMembers = teamResults.Sum(t => t.TeamSize)
            };

            if (individualResults.Any())
            {
                statistics.HighestIndividualScore = individualResults.Max(r => r.TotalScore);
                statistics.LowestIndividualScore = individualResults.Min(r => r.TotalScore);
                statistics.AverageIndividualScore = Convert.ToDouble(individualResults.Average(r => r.TotalScore));
                statistics.HighestGameIndividual = individualResults.Max(r => r.HighGame);

                // Статистика по играм
                var allIndividualGames = individualResults
                    .SelectMany(r => new[] { r.Game1, r.Game2, r.Game3, r.Game4, r.Game5, r.Game6 })
                    .Where(score => score > 0)
                    .ToList();

                if (allIndividualGames.Any())
                {
                    statistics.TotalStrikes = individualResults.Sum(r => r.StrikeCount);
                    statistics.TotalSpares = individualResults.Sum(r => r.SpareCount);
                    statistics.HighestSingleGame = allIndividualGames.Max();
                    statistics.PerfectGamesCount = allIndividualGames.Count(score => score == 300);
                }
            }

            if (teamResults.Any())
            {
                statistics.HighestTeamScore = teamResults.Max(r => r.TotalScore);
                statistics.LowestTeamScore = teamResults.Min(r => r.TotalScore);
                statistics.AverageTeamScore = Convert.ToDouble(teamResults.Average(r => r.TotalScore));
                statistics.AverageTeamSize = teamResults.Average(r => r.TeamSize);
            }

            return statistics;
        }

        #endregion
    }

    #region DTO классы

    public class TournamentResultsQueryDto
    {
        [Range(1, 1000)]
        public int Page { get; set; } = 1;

        [Range(1, 500)]
        public int PageSize { get; set; } = 50;

        public string? Search { get; set; }
        public int? MinPlace { get; set; }
        public int? MaxPlace { get; set; }
        public decimal? MinScore { get; set; }
        public ResultsType ResultsType { get; set; } = ResultsType.All;
    }

    public class ResultsQueryDto : TournamentResultsQueryDto
    {
        public string SortBy { get; set; } = "place";
        public bool Descending { get; set; } = false;
    }

    public class TournamentResultsDto
    {
        public long TournamentId { get; set; }
        public List<IndividualResultDto> IndividualResults { get; set; } = new();
        public List<TeamResultDto> TeamResults { get; set; } = new();
        public TournamentStatisticsDto Statistics { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class IndividualResultsDto
    {
        public long TournamentId { get; set; }
        public List<IndividualResultDto> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class TeamResultsDto
    {
        public long TournamentId { get; set; }
        public List<TeamResultDto> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class IndividualResultDto
    {
        public long Id { get; set; }
        public int Place { get; set; }
        public long PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string? PlayerRegion { get; set; }
        public Gender PlayerGender { get; set; }
        public decimal TotalScore { get; set; }
        public decimal AverageScore { get; set; }
        public int GamesPlayed { get; set; }

        // Детализация по играм
        public int Game1 { get; set; }
        public int Game2 { get; set; }
        public int Game3 { get; set; }
        public int Game4 { get; set; }
        public int Game5 { get; set; }
        public int Game6 { get; set; }

        // Статистика
        public int HighGame { get; set; }
        public int LowGame { get; set; }
        public int StrikeCount { get; set; }
        public int SpareCount { get; set; }
        public decimal Consistency { get; set; }

        // Метаданные
        public string? Notes { get; set; }
        public DateTime ResultDate { get; set; }

        // Вычисляемые свойства
        [NotMapped]
        public string GamesSummary => $"{Game1}/{Game2}/{Game3}/{Game4}/{Game5}/{Game6}";

        [NotMapped]
        public string FormattedAverage => AverageScore.ToString("F2");
    }

    public class TeamResultDto
    {
        public long Id { get; set; }
        public int Place { get; set; }
        public long TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? TeamAbbreviation { get; set; }
        public string? TeamRegion { get; set; }
        public decimal TotalScore { get; set; }
        public decimal AverageScore { get; set; }
        public int GamesPlayed { get; set; }
        public int TeamSize { get; set; }
        public decimal AveragePerMember { get; set; }

        // Детализация по играм
        public int Game1 { get; set; }
        public int Game2 { get; set; }
        public int Game3 { get; set; }
        public int Game4 { get; set; }
        public int Game5 { get; set; }
        public int Game6 { get; set; }

        // Участники команды
        public Dictionary<int, int> MemberScores { get; set; } = new();
        public List<TeamMemberDto> TeamMembers { get; set; } = new();

        // Метаданные
        public string? Notes { get; set; }
        public DateTime ResultDate { get; set; }

        // Вычисляемые свойства
        [NotMapped]
        public string GamesSummary => $"{Game1}/{Game2}/{Game3}/{Game4}/{Game5}/{Game6}";

        [NotMapped]
        public string FormattedAverage => AverageScore.ToString("F2");
    }

    public class TeamMemberDto
    {
        public long PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public TeamMemberRole Role { get; set; }
        public DateTime JoinedDate { get; set; }
        public int? JerseyNumber { get; set; }
        public decimal? AverageScore { get; set; }
        public int? ScoreInTournament { get; set; }
    }

    public class TournamentStatisticsDto
    {
        public TournamentType TournamentType { get; set; }
        public int TotalParticipants { get; set; }
        public int IndividualParticipants { get; set; }
        public int TeamParticipants { get; set; }
        public int TeamsCount { get; set; }
        public int TotalTeamsMembers { get; set; }

        // Индивидуальная статистика
        public decimal HighestIndividualScore { get; set; }
        public decimal LowestIndividualScore { get; set; }
        public double AverageIndividualScore { get; set; }
        public int HighestGameIndividual { get; set; }

        // Командная статистика
        public decimal HighestTeamScore { get; set; }
        public decimal LowestTeamScore { get; set; }
        public double AverageTeamScore { get; set; }
        public double AverageTeamSize { get; set; }

        // Общая статистика по играм
        public int TotalStrikes { get; set; }
        public int TotalSpares { get; set; }
        public int HighestSingleGame { get; set; }
        public int PerfectGamesCount { get; set; }
    }

    public enum ResultsType
    {
        All = 0,
        Individual = 1,
        Team = 2
    }

    #endregion

}