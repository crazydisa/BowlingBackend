using GamesResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppService service;

        public UsersController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/system/current-user")]
        public async Task<ActionResult<Models.User?>> GetCurrentUser()
        {
            return await service.GetCurrentUserAsync();
        }

        [HttpGet("/system/users")]
        public async Task<ActionResult<IEnumerable<Models.User>>> GetUsers()
        {
            return await service.GetAllByQueryAsync(service.Context.Users.Include(o => o.Roles));
        }
    }
}
