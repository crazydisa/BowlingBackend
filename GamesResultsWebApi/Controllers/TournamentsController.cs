
using GamesResults;
using GamesResults.Models.Bowling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BowlingStatistic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TournamentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TournamentsController(AppDbContext appContext, AppService service)
        {
            _context = appContext;
            //this.service = service;
        }
        // Получить необработанные турниры
        [HttpGet("unprocessed")]
        public async Task<IActionResult> GetUnprocessedTournaments([FromQuery] int limit = 50)
        {
            var tournaments = await _context.Tournaments
                .Include(t => t.City)
                .Where(t => !t.RatingsUpdated)
                .OrderByDescending(t => t.StartDate)
                .Take(limit)
                .Select(t => new TournamentDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    StartDate = t.StartDate,
                    City = t.City != null ? t.City.Name : null,
                    RatingsUpdated = t.RatingsUpdated,
                    PlayerCount = t.Results.Count
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TournamentDto>>.Success(tournaments));
        }

        // Получить последние турниры
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentTournaments([FromQuery] int limit = 10)
        {
            var tournaments = await _context.Tournaments
                .Include(t => t.City)
                .OrderByDescending(t => t.StartDate)
                .Take(limit)
                .Select(t => new TournamentDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    StartDate = t.StartDate,
                    City = t.City != null ? t.City.Name : null,
                    RatingsUpdated = t.RatingsUpdated,
                    RatingsUpdatedDate = t.RatingsUpdatedDate,
                    PlayerCount = t.Results.Count
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TournamentDto>>.Success(tournaments));
        }
    }

    public class TournamentDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public string? City { get; set; }
        public bool RatingsUpdated { get; set; }
        public DateTime? RatingsUpdatedDate { get; set; }
        public int PlayerCount { get; set; }
    }
}
