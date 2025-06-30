using GamesResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class ActionsController : ControllerBase
    {
        private readonly AppService service;

        public ActionsController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/system/actions")]
        public async Task<ActionResult<IEnumerable<Models.Action>>> GetActions()
        {
            return await service.Context.Actions
                .Include(o => o.ObjectType)
                .Include(o => o.ObjectType.RootContainer)
                .Include(o => o.Roles)
                .ToArrayAsync();
        }

        [HttpPut("/system/update-action-roles")]
        public async Task<ActionResult<Models.Action>> UpdateActionRoles(Models.Action action)
        {
            var updateQuery = service.GetActionQuery<Action>("UpdateActionRoles");
            if (updateQuery.IsAllowed)
            {
                var changeAction = service.Context.Actions
                    .Include(o => o.Roles)
                    .FirstOrDefault(o => o.Id == action.Id);

                var roleIds = action.Roles
                                .Select(o => o.Id)
                                .ToArray();

                var roles = service.Context.Roles
                                .Where(o => roleIds.Contains(o.Id))
                                .ToArray();

                if (changeAction != null)
                {
                    changeAction.Roles.Clear();
                    changeAction.Roles.AddRange(roles);

                    await service.Context.SaveChangesAsync();

                    return await service.Context.Actions
                        .Include(o => o.ObjectType)
                        .Include(o => o.ObjectType.RootContainer)
                        .Include(o => o.Roles)
                        .FirstOrDefaultAsync(o => o.Id == action.Id);
                }
                return NotFound();
            }
            return updateQuery.ContentResult;
        }
    }
}
