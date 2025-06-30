using GamesResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly AppService service;

        public SystemController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/system")]
        public async Task<ActionResult<Models.System>> GetSystem()
        {
            return await service.Context.System
                .FirstAsync();
        }
    }
}
