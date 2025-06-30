using GamesResults;
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly AppService service;

        public EventsController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/dictionaries/Events")]
        public async Task<ActionResult<IEnumerable<Models.Bowling.Event>>> GetEvents()
        {
            return await service.GetAllByQueryAsync(service.Context.Events
                .OrderBy(o => o.Title));
        }
    }
}
