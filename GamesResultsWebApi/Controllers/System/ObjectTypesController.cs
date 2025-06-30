using GamesResults;
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class ObjectTypesController : ControllerBase
    {
        private readonly AppService service;

        public ObjectTypesController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/system/object-types")]
        public async Task<ActionResult<IEnumerable<ObjectType>>> GetObjectTypes()
        {

            return await service.GetObjectTypes();
        }

        [HttpPut("/system/update-object-type")]
        public async Task<ActionResult<ObjectType>> UpdateObjectType(ObjectType type)
        {
            var objType = service.Context.ObjectTypes
                .Include(o => o.Properties)
                .FirstOrDefault(o => o.Id == type.Id);

            if (objType != null)
            {
                objType.Title = type.Title;
                foreach (var prop in type.Properties)
                {
                    var objProp = service.Context.ObjectProperties.FirstOrDefault(o => o.Id == prop.Id);
                    if (objProp != null)
                    {
                        objProp.Title = prop.Title;
                        objProp.DataFormat = prop.DataFormat;
                        objProp.DisplayExpr = prop.DisplayExpr;
                    }
                }
                return await service.UpdateOneAsync(objType.Id, objType);
            }
            return NotFound();
        }
    }
}
