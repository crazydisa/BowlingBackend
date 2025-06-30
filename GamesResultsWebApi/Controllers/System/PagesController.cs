using GamesResults;
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamesResults.Interfaces;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class PagesController : ControllerBase
    {
        private readonly AppService service;

        public PagesController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/system/pages")]
        public async Task<ActionResult<IEnumerable<Page>>> GetPages()
        {
            return await service.Context.Pages

                .OrderBy(page => page.SortIndex)
                .Include(page => page.LoadAction).ToArrayAsync();
            //return await service.GetAllByQueryAsync(service.Context.Pages
            //    .OrderBy(page => page.SortIndex)
            //    .Include(page => page.LoadAction)
            //);
        }
    }
}
