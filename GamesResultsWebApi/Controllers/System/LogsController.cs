using GamesResults;
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly AppService service;

        public LogsController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/system/logs")]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogs(DateTime? from, DateTime? to)
        {
            return await service.GetAllByQueryAsync(service.Context.Logs
                .Where(o => (from == null || o.LogTime >= from) && (to == null || o.LogTime <= to))
                .Include(o => o.Object)
                .Include(o => o.Action)
                .Include(o => o.Action.ObjectType)
                .Include(o => o.Action.ObjectType.RootContainer)
                .Include(o => o.User)
                .OrderByDescending(o => o.LogTime)
            );
        }

        [HttpGet("/system/log-details")]
        public async Task<ActionResult<IEnumerable<LogDetail>>> GetLogDetails(Guid id)
        {
            return await service.GetAllByQueryAsync(service.Context.LogDetails
                .Where(o => o.LogId == id)
            );
        }
    }
}
