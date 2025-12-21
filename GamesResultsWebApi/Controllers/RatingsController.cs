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
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(IRatingService ratingService, ILogger<RatingsController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        [HttpPost("tournament/{tournamentId}/update")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> UpdateTournamentRatings(int tournamentId)
        {
            try
            {
                await _ratingService.UpdateRatingsAfterTournamentAsync(tournamentId);
                return Ok(new { Success = true, Message = "Рейтинги успешно обновлены" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении рейтингов турнира {TournamentId}", tournamentId);
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("player/{playerId}")]
        public async Task<IActionResult> GetPlayerRating(int playerId)
        {
            try
            {
                var rating = await _ratingService.GetPlayerRatingAsync(playerId);
                return Ok(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении рейтинга игрока {PlayerId}", playerId);
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("global")]
        public async Task<IActionResult> GetGlobalRankings([FromQuery] int top = 100)
        {
            try
            {
                var rankings = await _ratingService.GetGlobalRankingsAsync(top);
                return Ok(rankings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении глобального рейтинга");
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("predict")]
        public async Task<IActionResult> PredictMatch(
            [FromQuery] int player1Id,
            [FromQuery] int player2Id,
            [FromQuery] TournamentType tournamentType = TournamentType.Individual)
        {
            try
            {
                var prediction = await _ratingService.PredictMatchAsync(player1Id, player2Id, tournamentType);
                return Ok(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при прогнозе матча {Player1Id} vs {Player2Id}", player1Id, player2Id);
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}