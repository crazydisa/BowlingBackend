using GamesResults;
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppService service;

        public RolesController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/system/roles")]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await service.GetAllByQueryAsync(service.Context.Roles
                .OrderBy(role => role.Title)
                .Include(role => role.Actions)
            );
        }
    }
}
