
using Microsoft.AspNetCore.Mvc;


namespace GamesResults.Controllers.System
{
    [ApiController]
    public class TournamentsController : ControllerBase
    {
        private readonly AppService service;

        public TournamentsController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/dictionaries/Tournaments")]
        public async Task<ActionResult<IEnumerable<Models.Bowling.Tournament>>> GetTournaments()
        {
            return await service.GetAllByQueryAsync(service.Context.Tournaments
                .OrderBy(o => o.Title));
        }
    }
}
