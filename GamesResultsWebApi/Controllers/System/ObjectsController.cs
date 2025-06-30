using GamesResults;
using Microsoft.AspNetCore.Mvc;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class ObjectsController : ControllerBase
    {
        private readonly AppService service;

        public ObjectsController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/system/objects")]
        public async Task<ActionResult<IEnumerable<Models.Object>>> GetObjects(string? objs)
        {
            if (objs != null)
            {
                long[] objsIds = objs.Split(',').Select(o => Convert.ToInt64(o)).ToArray();
                return await service.GetAllByQueryAsync(service.Context.Objects
                    .Where(o => objsIds.Any(id => id == o.Id)));
            }
            else
            {
                return await service.GetAllByQueryAsync(service.Context.Objects);
            }
        }
    }
}
