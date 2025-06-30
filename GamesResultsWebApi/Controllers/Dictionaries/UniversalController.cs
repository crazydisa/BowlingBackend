
using GamesResults;
using GamesResults.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
using System.Reflection;
//using System.Web;
//using System.Web.Http;
using Microsoft.Net.Http.Headers;
using System.Net;
//using PirAppLib.Models.Smpd;
using System.Text.Json;
using Microsoft.Extensions.Logging;
//using SapsanLibPost.Models;
using System;
using System.Net.Mail;
using System.Resources;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using GamesResults.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.CodeDom;
using System.IO;
using OfficeOpenXml;
using System.Threading;
using GamesResults.Utils;
using System.Text.RegularExpressions;

namespace GamesResults.Controllers.System
{
    [ApiController]
    
    public class UniversalController : ControllerBase
    {

        private readonly AppDbContext nsContext;
        private readonly AppService service;

       
        public UniversalController(AppDbContext appContext, AppService service)
        {
            nsContext = appContext;
            this.service = service;
        }
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [HttpPost("/universa/upload-data")]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var form = await this.Request.ReadFormAsync();
                var turnir = form["turnir"].ToString();
                var file = form.Files.GetFile("pdfFile");
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Файл не предоставлен");
                }
                if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                {
                    return BadRequest("Только PDF файлы разрешены");
                }
                var filePath = Path.Combine("Uploads", Guid.NewGuid().ToString() + ".pdf");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                //Здесь обрабатываю данные
                return Ok(new { Message = "Файл и данные успешно получены" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }




        [HttpPut("/universal/setOne2MAnyObjects")]
        public async Task<ActionResult<object>> SetOne2MAnyObjects(object obj)
        {
            var value = obj;
            var jsonDoc = JsonSerializer.Serialize(value);
            One2Many2 o2m;
            try
            {
                o2m = JsonSerializer.Deserialize<One2Many2>(jsonDoc);
            }
            catch
            {
                return new ContentResult() { StatusCode = (int)HttpStatusCode.BadRequest };
            }
            var oneTypeName = service.GetStructureNameSpace(o2m.OneTypeName);
            oneTypeName.FillMissingDefaultValues();
            var manyTypeName = service.GetStructureNameSpace(o2m.ManyTypeName);
            manyTypeName.FillMissingDefaultValues();
            var oneObl = o2m.OneObject;
            var manyObl = o2m.ManyObject;
            dynamic? oneInstance = service.GetInstance(oneTypeName);
            dynamic? manyInstance = service.GetInstance(manyTypeName);
            if (oneInstance != null && manyInstance != null)
            {
                var deserializeOne = service.Deserialize(oneInstance, oneObl);
                
                
                var IdPropOne = deserializeOne.GetType().GetProperty("Id");
                var idValOne = IdPropOne?.GetValue(deserializeOne);
                var deletedAtPropOne = deserializeOne.GetType().GetProperty("DeletedAt");
                var deletedAtValOne = deletedAtPropOne?.GetValue(deserializeOne);
                object[] subordinateObjs;
                if (deletedAtValOne != null)
                {
                    RequestOption requestOption = new RequestOption();
                    requestOption.useFilterIds = false;
                    requestOption.condition = "TabelId == " + idValOne + " && DeletedAt == null";
                    var manyFromBase = await FetchFromTable(manyInstance, nsContext, requestOption);
                    subordinateObjs = new object[manyFromBase.Count];
                    int i = 0;
                    foreach (var row in manyFromBase)
                    {
                        row.GetType().GetProperty("DeletedAt")?.SetValue(row, deletedAtValOne, null);
                        subordinateObjs[i] = row;
                        i++;
                    }

                }
                else
                {
                    var deserializeMany = service.DeserializeList(manyInstance, manyObl);
                    int i = 0;
                    foreach (var subObj in deserializeMany)
                    {
                        subObj.GetType().GetProperty("TabelId").SetValue(subObj, idValOne, null);


                        i++;
                    }
                    subordinateObjs = new object[i];
                    i = 0;
                    foreach (var subObj in deserializeMany)
                    {
                        subordinateObjs[i] = subObj;
                        i++;
                    }
                    deserializeOne.GetType().GetProperty("CardClocks").SetValue(deserializeOne, deserializeMany, null);
                }

                object[] subbObjs = service.SetEntryInTable(manyInstance, subordinateObjs);

                // var firstNavigationProp = service
                
                //await nsContext.SaveChangesAsync();
                object[] parentObj = new object[1];
                parentObj[0] = deserializeOne;
                object[] mainbObj =  service.SetEntryInTable(oneInstance, parentObj);

                await nsContext.SaveChangesAsync();
                if (deletedAtValOne == null)
                {
                    //var deserializeMany = service.DeserializeList(manyInstance, subbObjs);
                    var filteredSubbObjs = subbObjs.Where(o => o.GetType().GetProperty("DeletedAt")?.GetValue(o) != null ? false : true).ToArray();
                    //int i = 0;
                    //foreach (var subObj in subbObjs)
                    //{
                    //    var deletedAt = subObj.GetType().GetProperty("DeletedAt");
                    //    var val = deletedAt?.GetValue(subObj);
                    //    if (val != null)
                    //    { 

                    //    }
                    //    i++;
                    //}
                    
                    One2Many2 result = new One2Many2();
                    result.OneObject = mainbObj;
                    result.ManyObject = filteredSubbObjs;
                    result.OneTypeName = oneTypeName.typeName;
                    result.OneTypeNameSpace = oneTypeName.nameSpace;
                    result.ManyTypeName = manyTypeName.typeName;
                    result.ManyTypeNameSpace = manyTypeName.nameSpace;
                    result.RequestOption.actionName = "add";
                    return result;
                }
                else
                {
                    One2Many2 result = new One2Many2();
                    result.OneObject = mainbObj;
                    result.ManyObject = subbObjs;
                    result.OneTypeName = oneTypeName.typeName;
                    result.OneTypeNameSpace = oneTypeName.nameSpace;
                    result.ManyTypeName = manyTypeName.typeName;
                    result.ManyTypeNameSpace = manyTypeName.nameSpace;
                    result.RequestOption.actionName = "remove";
                    return result;
                }
            }
            else
                return new ContentResult() { StatusCode = (int)HttpStatusCode.BadRequest };
            //return new ContentResult() { StatusCode = (int)HttpStatusCode.OK };
        }
       
        [HttpPut("/universal/updateAnyObjects/{typeName}")]
        public async Task<ActionResult<object>> UpdateAnyObjects(string typeName, object obj)
        {
            var structNameSpace = service.GetStructureNameSpace(typeName);
            structNameSpace.FillMissingDefaultValues();
            List<Type> dbsetTypes = FetchDbSetTypes(nsContext);

            foreach (var dbsetType in dbsetTypes)
            {
                try
                {
                    if (dbsetType.Name == structNameSpace.typeName && dbsetType.Namespace == structNameSpace.nameSpace)
                    {
                        Type myType = typeof(Microsoft.EntityFrameworkCore.Internal.InternalDbSet<>).MakeGenericType(dbsetType);
                        dynamic instance = Activator.CreateInstance(myType, nsContext, dbsetType.Name);
                        var result = await updateEntryInTable(instance, dbsetType, obj);
                        if (result != null)
                        {
                            var IdProp = result.GetType().GetProperty("DeletedAt");
                            var idVal = IdProp?.GetValue(result);
                            if (idVal != null)
                            {
                                return new ContentResult() { StatusCode = (int)HttpStatusCode.OK };
                            }
                        }
                        else
                        {
                            return new ContentResult() { StatusCode = (int)HttpStatusCode.BadRequest };
                        }
                        return result;
                    };
                }
                catch (Exception)
                {
                    return new ContentResult() { StatusCode = (int)HttpStatusCode.BadRequest };
                }
            }
            return new ContentResult() { StatusCode = (int)HttpStatusCode.BadRequest };
        }
        private async Task<T?> updateEntryInTable<T>(DbSet<T> _, Type dbsetType, object value) where T : class, IDeleted, ICreated, IEditable, ITitled
        {
            var objType = nsContext.ObjectTypes.Include(o => o.Properties).SingleOrDefault(o => o.Name == dbsetType.Name);
            var jsonDoc = JsonSerializer.Serialize(value);
            T newRow;
            try
            {
                newRow = JsonSerializer.Deserialize<T>(jsonDoc);
            }
            catch
            {
                return null;
            }
            var IdProp = newRow.GetType().GetProperty("Id");
            var idVal = IdProp?.GetValue(newRow);
            var oldRow = nsContext.Set<T>().Find(idVal);
            if (newRow.Title == null)
            {
                if (objType?.DisplayExpr != null)
                {
                    var displayExprProp = newRow.GetType().GetProperty(objType.DisplayExpr);
                    if (displayExprProp != null)
                    {
                        var displayExprVal = displayExprProp.GetValue(newRow);
                        if (displayExprVal != null)
                            newRow.Title = displayExprVal.ToString();
                    }
                }
            }
            if (newRow.Title == null)
                newRow.Title = dbsetType.Name;
            var user = service.GetCurrentUser();
            EntityEntry addedRow;
            if (oldRow != null)
            {
                if (newRow.DeletedAt != null)
                {
                    newRow.DeleterId = user.Id;
                }
                else
                {
                    newRow.EditorId = user.Id;
                    newRow.ModifiedAt = DateTime.Now;
                }
                nsContext.Entry(oldRow).CurrentValues.SetValues(newRow);
            }
            else
            {
                newRow.AuthorId = user.Id;
                newRow.CreatedAt = DateTime.Now;
                newRow.ModifiedAt = DateTime.Now;
                addedRow = nsContext.Set<T>().Add(newRow);
            }
            await nsContext.SaveChangesAsync();
            //DbSet<T> parameter is not needed - it will throw an Exception
            return (T)newRow;
        }

        [HttpPost("/universal/anyObjects")]
        public async Task<ActionResult<IEnumerable<object>>> GetAnyObjects(RequestOption requestOption)
        {
            var jsonDoc = JsonSerializer.Serialize(requestOption);
            RequestOption arguments;
            try
            {
                arguments = JsonSerializer.Deserialize<RequestOption>(jsonDoc);
            }
            catch
            {
                return new ContentResult() { StatusCode = (int)HttpStatusCode.BadRequest };
            }
            if (arguments.useFilterIds == true)
            {
                Type? unknown = Type.GetType(requestOption.idPropTypeName);
                jsonDoc = JsonSerializer.Serialize(arguments.ids);
                object[] jsonIds;
                object test = Guid.NewGuid();
                try
                {
                    jsonIds = JsonSerializer.Deserialize<object[]>(jsonDoc);
                }
                catch
                {
                    return new ContentResult() { StatusCode = (int)HttpStatusCode.BadRequest };
                }
                int i = 0;
                object[] desIds = new object[jsonIds.Length];
                foreach (var item in jsonIds)
                {
                    jsonDoc = JsonSerializer.Serialize(item);
                    desIds[i] = JsonSerializer.Deserialize(jsonDoc, unknown);
                    i++;
                }
                arguments.ids = desIds;
            }

            DbContext? context = service.GetDbContextByNameSpace(arguments.nameSpace);
            List<Type> dbsetTypes = FetchDbSetTypes(context);
            foreach (var dbsetType in dbsetTypes)
            {
                try
                {
                    if (dbsetType.Name == arguments.typeName)
                    {
                        if (typeof(IDeleted).IsAssignableFrom(dbsetType))
                            arguments.condition = string.IsNullOrWhiteSpace(arguments.condition) || arguments.condition == "null"? "DeletedAt == null": arguments.condition.Contains("DeletedAt")? arguments.condition: arguments.condition + "&& DeletedAt == null";
                        else
                            arguments.condition = string.IsNullOrWhiteSpace(arguments.condition) || arguments.condition == "null" ? "true" : arguments.condition;
                        Type myType = typeof(Microsoft.EntityFrameworkCore.Internal.InternalDbSet<>).MakeGenericType(dbsetType);
                        dynamic instance = Activator.CreateInstance(myType, context, dbsetType.Name);
                        return  await FetchFromTable(instance, context, arguments);
                    };
                }
                catch (Exception)
                {
                    return new ContentResult() { StatusCode = (int)HttpStatusCode.BadRequest };
                }
            }
            return new ContentResult() { StatusCode = (int)HttpStatusCode.NoContent };
            //Cast<object>());
        }
        static T Cast<T>(object obj, T Type)
        {
            return (T)obj;
        }
        public T GetInstance<T>(string type)
        {
            return (T)Activator.CreateInstance(Type.GetType(type));
        }
        private async Task<List<T>> FetchFromTable<T>(DbSet<T> _, DbContext dbContext, RequestOption arguments) where T : class
        {
            
            
                //object unknownObject = arguments.ids;
                //Type targetType = typeof(IEnumerable<>).MakeGenericType(typeof(object));
                //var enumerable = unknownObject as IEnumerable<Guid>;
                //var seqType = typeof(Guid);//.GetGenericArguments()[0];
                //Type type = typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
                //Type? unknown = Type.GetType("Guid");
                ////var guid = GetInstance(unknown);
                //dynamic unknownInstance = Activator.CreateInstance(unknown);
                //unknownInstance = arguments.ids[0];
                //Guid[] ids = arguments.ids as Guid[];
                
            if (arguments.useFilterIds == true)
            {
                object ids;
                if(arguments.idPropTypeName == "System.Guid")
                {
                    Guid[]? guidIds = new Guid[arguments.ids.Length];
                    for (var i = 0; i < arguments.ids.Length; i++)
                    {
                        guidIds[i] = (Guid)arguments.ids[i];
                    }
                    ids = guidIds;
                }
                else
                {
                    long[]? longIds = new long[arguments.ids.Length];
                    for (var i = 0; i < arguments.ids.Length; i++)
                    {
                        longIds[i] = (long)arguments.ids[i];
                    }
                    ids = longIds;

                }
                
                arguments.condition = arguments.condition == "true" ? arguments.idPropName + $" in @0" : arguments.condition + " && " + arguments.idPropName + $" in @0";
                return await dbContext.Set<T>().Where(arguments.condition, ids).ToListAsync();
            }
            else
                return await dbContext.Set<T>().Where(arguments.condition).ToListAsync();
        }
        private List<Type> FetchDbSetTypes(DbContext context)
        {
            var properties = context.GetType().GetProperties();
            var dbSets = new List<Type>();
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                if (propertyType.IsGenericType && propertyType.Name.ToLower().Contains("dbset"))
                {
                    Type dbSetType = propertyType.GenericTypeArguments[0]; //point of interest here
                    dbSets.Add(dbSetType);
                }
            }
            return dbSets;
        }

       
    }
}
