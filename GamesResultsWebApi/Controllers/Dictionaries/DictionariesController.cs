
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamesResults.Controllers.System
{
    [ApiController]
    public class DictionariesController : ControllerBase
    {
        private readonly AppService service;

        public DictionariesController(AppService service)
        {
            this.service = service;
        }

        [HttpGet("/dictionaries/dictionary-types")]
        public async Task<ActionResult<IEnumerable<DictionaryType>>> GetDictionaryTypes()
        {
            return await service.GetAllByQueryAsync(service.Context.DictionaryTypes
                //.Where(o => !o.IsDeleted)
                .OrderBy(o => o.SortIndex).ThenBy(o => o.Title)
                .Include(o => o.Parent));
        }

        [HttpGet("/dictionaries/dictionary-items")]
        public async Task<ActionResult<IEnumerable<DictionaryItem>>> GetDictionaryItems(string? typeName, long? typeId)
        {
            if (typeId == null && typeName != null)
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    return await service.GetAllByQueryAsync(service.Context.DictionaryItems
                        //.Where(o => !o.IsDeleted)
                        .OrderBy(o => o.SortIndex).ThenBy(o => o.Title)
                        .Include(o => o.DictionaryType));
                }
                else
                {
                    var dictionaryType = service.Context.DictionaryTypes.FirstOrDefault(o => o.Name == typeName);
                    if (dictionaryType != null)
                    {
                        return await service.GetAllByQueryAsync(service.Context.DictionaryItems
                            .Where(o => o.DictionaryTypeId == dictionaryType.Id)
                            .OrderBy(o => o.SortIndex).ThenBy(o => o.Title)
                            .Include(o => o.DictionaryType));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            else if (typeId != null && typeName==null)
            {
                return await service.GetAllByQueryAsync(service.Context.DictionaryItems
                        .Where(o => o.DictionaryTypeId == typeId)
                        .OrderBy(o => o.SortIndex).ThenBy(o => o.Title)
                        .Include(o => o.DictionaryType));
            }
            else
            {
                return await service.GetAllByQueryAsync(service.Context.DictionaryItems
                       //.Where(o => !o.IsDeleted)
                       .OrderBy(o => o.SortIndex).ThenBy(o => o.Title)
                       .Include(o => o.DictionaryType));
            }
        }

        [HttpPost("/dictionaries/create-dictionary-item")]
        public async Task<ActionResult<DictionaryItem>> CreateDictionaryItem(DictionaryItem item)
        {
            return await service.CreateOneAsync(item);
        }

        [HttpPut("/dictionaries/update-dictionary-item")]
        public async Task<ActionResult<DictionaryItem>> UpdateDictionaryItem(DictionaryItem item)
        {
            return await service.UpdateOneAsync(item.Id, item);
        }

        [HttpDelete("/dictionaries/delete-dictionary-item")]
        public async Task<ActionResult> DeleteDictionaryItem(DictionaryItem item)
        {
            return await service.DeleteOneAsync<DictionaryItem>(item.Id);
        }

    }
}
