using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GamesResults.Models;
using System.Net;
using System.Runtime.CompilerServices;
//using SapsanLib;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using GamesResults.Interfaces;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamesResults
{
    public struct StructNameSpace
    {
        public string? typeName=null;
        public string? rootNameSpace=null;
        public string? nameSpace = null;
        public string? fullName = null;
        public StructNameSpace(string name)
        {
            typeName = name;
            var index = name.LastIndexOf(".");
            if (index > 0 && index < name.Length - 1)
            {
                typeName = name.Substring(index + 1, name.Length - 1 - index);

                var lastDot = index;
                index = name.IndexOf(".");
                if (index > 0)
                {
                    rootNameSpace = name.Substring(0, index);
                    nameSpace = name.Substring(0, lastDot);
                }
            }
            fullName = nameSpace + "." + typeName;
        }
        public void FillMissingDefaultValues()
        {
            if (string.IsNullOrWhiteSpace(nameSpace))
            {
                nameSpace = "GamesResults.Models.Bowling";
                rootNameSpace = "GamesResults";
                fullName = nameSpace + "." + typeName;
            }
        }
    }
    public class ActionQuery
    {
        public Models.User? User { get; private set; }
        public Models.Action? Action { get; private set; }
        public int? LogLevel { get; private set; }
        public object? Object { get; private set; }
        public string OldValue { get; private set; }
        public string NewValue { get; private set; }
        public ContentResult ContentResult { get; private set; }
        public string Message { get; private set; }
        public bool IsAllowed { get; private set; }

        public ActionQuery(Models.User? user, Models.Action? action, int? logLevel, string message, HttpStatusCode httpStatus)
        {
            User = user;
            Action = action;
            LogLevel = logLevel;
            Message = message;
            ContentResult = new ContentResult
            {
                Content = JsonSerializer.Serialize(new { message }),
                ContentType = "application/json",
                StatusCode = (int)httpStatus
            };
            IsAllowed = (httpStatus == HttpStatusCode.OK);
        }

        public void SetOldValue(object? oldValue)
        {
            if (oldValue != null)
            {
                Object = oldValue;
                OldValue = JsonSerializer.Serialize(oldValue, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
            }
        }

        public void SetNewValue(object? newValue)
        {
            if (newValue != null)
            {
                Object = newValue;
                NewValue = JsonSerializer.Serialize(newValue, new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
            }
        }
    }

    public class AppService
    {
        private readonly IConfiguration config;
        private readonly AppDbContext context;
        //private readonly SapsanDbContext sapsanContext;
        private readonly IHttpContextAccessor accessor;
        public DbContext userIdentityContext;
        public AppDbContext Context => context;
        //public SapsanDbContext SapsanContext => sapsanContext;
        public string? UserName => accessor?.HttpContext?.User?.Identity?.Name;

        public AppService(IConfiguration config, AppDbContext context, /*SapsanDbContext sapsanContext,*/ IHttpContextAccessor accessor)
        {
            this.config = config;
            this.context = context;
            this.accessor = accessor;
            //this.sapsanContext = sapsanContext;
            userIdentityContext = context;
            ////context.Actions.Add(new PirAppBp.Models.Action()
            ////{
            ////    Name = "GetComplectById",
            ////    Title = "Получить комплект по id",
            ////});
            ////context.SaveChanges();
            ////context.Actions.Add(new PirAppBp.Models.Action()
            ////{
            ////    Name = "GetComplects",
            ////    Title = "Получить комплекты",
            ////});
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetDocumentById",
            //    Title = "Получить документ по id",
            //});
            //context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetDocumentPartById",
            //    Title = "Получить часть документа по id",
            //});
            //context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetDocuments",
            //    Title = "Получить части документов",
            //});
            //context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetContractById",
            //    Title = "Получить проект по id",
            //});
            //context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetContracs",
            //    Title = "Получить проекты",
            //});
            //context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetDocumentParts",
            //    Title = "Получить части документов ",
            //});
            //context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetOpById",
            //    Title = "Получить КО  по id",
            //});
            //context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetStageById",
            //    Title = "Получить стадию по id",
            //});
            //context.SaveChanges();
            ////context.Actions.Add(new PirAppBp.Models.Action()
            ////{
            ////    Name = "GetMarks",
            ////    Title = "Получить марки",
            ////});
            ////context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetMarkById",
            //    Title = "Получить марку по id",
            //});
            //context.SaveChanges();
            ////context.Actions.Add(new PirAppBp.Models.Action()
            ////{
            ////    Name = "GetOis",
            ////    Title = "Получить объект инфраструктуры",
            ////});
            ////context.SaveChanges();
            ////context.Actions.Add(new PirAppBp.Models.Action()
            ////{
            ////    Name = "GetOiById",
            ////    Title = "Получить объект инфраструктуры по id",
            ////});
            ////context.SaveChanges();
            //context.Actions.Add(new PirAppBp.Models.Action()
            //{
            //    Name = "GetFieldById",
            //    Title = "Получить месторождение по id",
            //});
            //context.SaveChanges();
            //var admRole = context.Roles.Where(o => o.Name == "Admins").FirstOrDefault();
            //context.Users.Add(new User()
            //{
            //    Name = "TNNC-\\Администратор",
            //    Title = "Барулин Д.Б.",
            //    Email = "dbbarulin@tnnc.rosneft.ru",
            //    Roles = new List<Role> { admRole }
            //});
            //context.SaveChanges();
        }


        public IConfigurationSection GetConfigSection(string key)
        {
            return this.config.GetSection(key);
        }

        public IConfigurationSection GetConfigSection(string key, string subKey)
        {
            return this.config.GetSection(string.Format("{0}:{1}", key, subKey));
        }
        public DbContext? GetDbContextByNameSpace(string? nameSpace)
        {
            if (string.IsNullOrEmpty(nameSpace)) return null;
            DbContext? context = null;
            string rootNameSpace = GetRootNameSpace(nameSpace);
            var members = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            IEnumerable<FieldInfo> memDbContext = from member in members
                                                  where member.FieldType.IsAssignableTo(typeof(DbContext)) && member.FieldType.Namespace == rootNameSpace
                                                  select member;
            if (memDbContext.Any())
            {
                if (memDbContext.FirstOrDefault()?.GetValue(this) is DbContext findedContext)
                context = findedContext;
            }
            return context;
        }
        public StructNameSpace GetStructureNameSpace(string nameSpace)
        {
            StructNameSpace structNameSpace = new StructNameSpace(nameSpace);
            return structNameSpace;
        }
        public object? GetNewReflected(StructNameSpace structNameSpace)
        {
            if (string.IsNullOrWhiteSpace(structNameSpace.fullName))
                return null;
            var reflectedFirst = Type.GetType(structNameSpace.fullName);
            if (reflectedFirst == null) return null;
            var constr = reflectedFirst.GetConstructor(new Type[0]);
            if(constr == null) return null;
            var reflectedObj = constr.Invoke(new object[0]);
            return reflectedObj;
        }
        public  T? Deserialize<T>(DbSet<T> _, object obj) where T : class, ITitled
        {
            var jsonDoc = JsonSerializer.Serialize(obj);
            T tObj;
            try
            {
                tObj = JsonSerializer.Deserialize<T>(jsonDoc);
            }
            catch
            {
                return null;
            }
            if (tObj == null) return null;
            tObj.Title = GetTitle<T>(tObj);
            return tObj;
        }
        public List<T>? DeserializeList<T>(DbSet<T> _, object[] objs) where T : class, ITitled, IDeleted
        {
            var jsonDoc = JsonSerializer.Serialize(objs);
            List<T> tObjs;
            var jsonOptions = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            try
            {
                tObjs = JsonSerializer.Deserialize<List<T>>(jsonDoc, jsonOptions);
            }
            catch
            {
                return null;
            }
            foreach (var obj in tObjs)
            {
                obj.Title = GetTitle<T>(obj);
                
            }
           
            return tObjs;
        }
        public object? GetInstance(StructNameSpace structNameSpace)
        {
            List<Type> dbsetTypes = FetchDbSetTypes(context);
            foreach (var dbsetType in dbsetTypes)
            {
                try
                {
                    if (dbsetType.Name == structNameSpace.typeName && dbsetType.Namespace == structNameSpace.nameSpace)
                    {
                        Type myType = typeof(Microsoft.EntityFrameworkCore.Internal.InternalDbSet<>).MakeGenericType(dbsetType);
                        dynamic instance = Activator.CreateInstance(myType, context, dbsetType.Name);
                        return instance;
                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
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
        public string? FindNavigationProperty()
        {
            string? result=null;

            return result;
        }
        public string GetTitle<T>(object obj) where T : class, ITitled
        {
            T value = (T)obj;
            var typeName = value.GetType().Name;
            string title = value.Title;
            if (string.IsNullOrEmpty(title))
            {
                var objType = context.ObjectTypes.Include(o => o.Properties).SingleOrDefault(o => o.Name == typeName);
                if (objType?.DisplayExpr != null)
                {
                    var displayExprProp = value.GetType().GetProperty(objType.DisplayExpr);
                    if (displayExprProp != null)
                    {
                        var displayExprVal = displayExprProp.GetValue(value);
                        if (displayExprVal != null)
                            title = displayExprVal.ToString();
                    }
                }
                
            }
            if (title == null)
                title = typeName;
            return title;
        } 
        public  object[] SetEntryInTable<T>(DbSet<T> _, object[] objs, bool added = true) where T : class, IDeleted, ICreated, IEditable, ITitled
        {
            int num = objs.Length;
            object[] list = new object[num];
            int i = 0;
            foreach (var obj in objs)
            {
                T value = (T)obj;
                var typeName = value.GetType().Name;
                var objType = context.ObjectTypes.Include(o => o.Properties).SingleOrDefault(o => o.Name == typeName);
                var IdProp = value.GetType().GetProperty("Id");
                var idVal = IdProp?.GetValue(value);
                var oldRow = context.Set<T>().Find(idVal);
                value.Title = GetTitle<T>(value);
                var user = GetCurrentUser();
                EntityEntry addedRow;
                if (oldRow != null)
                {
                    if (value.DeletedAt != null)
                    {
                        value.DeleterId = user.Id;
                        list[i] = value;
                    }
                    else
                    {
                        value.EditorId = user.Id;
                        value.ModifiedAt = DateTime.Now;
                        list[i] = value;
                    }
                    
                    context.Entry(oldRow).CurrentValues.SetValues(value);
                    var currentState = context.Entry(oldRow).State;
                    if (currentState == EntityState.Unchanged)
                    {
                        context.Entry(oldRow).State = EntityState.Modified;
                    }
                }
                else if(added)
                {
                    value.AuthorId = user.Id;
                    value.CreatedAt = DateTime.Now;
                    value.ModifiedAt = DateTime.Now;
                    addedRow = context.Set<T>().Add(value);
                    list[i] = value;
                }
                i++;
            }
            //await context.SaveChangesAsync();
            //DbSet<T> parameter is not needed - it will throw an Exception
            return list;
        }
        public string GetRootNameSpace(string nameSpace)
        {
            string rootNameSpace = nameSpace;
            var index = nameSpace.IndexOf(".");
            if (index != -1)
            {
                rootNameSpace = nameSpace.Substring(0, index);
            }
            return rootNameSpace;
        }
        public bool SetUserIdentityContext(string nameSpace)
        {
            var someContext= GetDbContextByNameSpace(nameSpace);
            if (someContext != null)
            {
                userIdentityContext = someContext;
                return true;
            }
            else
                return false;
        }
        public Models.User GetCurrentUser()
        {
            return context.Users
                    .Include(u => u.Roles)
                    .First(u => u.Login == UserName);
        }
        //public T GetCurrentUserInContext<T>() where T : class
        //{
        //    return userIdentityContext.Set<T>()

        //            .First(u => u.Name == UserName);
        //}
        public async Task<ActionResult<Models.User?>> GetCurrentUserAsync()
        {
            if (UserName != null && context.Users.Any(u => u.Login == UserName))
            {
                return await context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Login == UserName);
            }
            else
            {
                return new ContentResult
                {
                    Content = JsonSerializer.Serialize(new { message = "Пользователь не найден" }),
                    ContentType = "application/json",
                    StatusCode = 404
                };
            }
        }

        public async Task<ActionResult<IEnumerable<T>>> GetAllByQueryAsync<T>(IQueryable<T> queryable, [CallerMemberName] string actionName = "") where T : class
        {
            var actionQuery = GetActionQuery<T>(actionName);
            if (actionQuery.IsAllowed)
            {
                return await queryable
                    .ToArrayAsync();
            }
            else
            {
                return actionQuery.ContentResult;
            }
        }

        public async Task<ActionResult<T>> GetOneByIdAsync<T>(long id, [CallerMemberName] string actionName = "") where T : class
        {
            var actionQuery = GetActionQuery<T>(actionName);
            if (actionQuery.IsAllowed)
            {
                return await context.Set<T>()
                    .FindAsync(id);
            }
            else
            {
                return actionQuery.ContentResult;
            }
        }

        public async Task<ActionResult<T>> GetOneByQueryAsync<T>(IQueryable<T> queryable, [CallerMemberName] string actionName = "") where T : class
        {
            var actionQuery = GetActionQuery<T>(actionName);
            if (actionQuery.IsAllowed)
            {
                return await queryable
                    .FirstOrDefaultAsync();
            }
            else
            {
                return actionQuery.ContentResult;
            }
        }

        public async Task<ActionResult<T>> CreateOneAsync<T>(T value, [CallerMemberName] string actionName = "") where T : class
        {
            var actionQuery = GetActionQuery<T>(actionName);
            if (actionQuery.IsAllowed)
            {
                SetObjectType(ref value);
                SetObjectAuthor(actionQuery, ref value);
                context.Set<T>().Add(value);
                await context.SaveChangesAsync();
                SetNewValue(actionQuery, value);
                await AddActionLogAsync<T>(actionQuery);
                return value;
            }
            else
            {
                return actionQuery.ContentResult;
            }
        }

        public async Task<ActionResult<T>> UpdateOneAsync<T>(long id, T value, [CallerMemberName] string actionName = "") where T : class
        {
            var actionQuery = GetActionQuery<T>(actionName);
            if (actionQuery.IsAllowed)
            {
                var oldValue = context.Set<T>().Find(id);
                SetOldValue(actionQuery, oldValue);
                SetObjectEditor(actionQuery, ref oldValue);
                context.Entry(oldValue).CurrentValues.SetValues(value);
                await context.SaveChangesAsync();
                SetNewValue(actionQuery, oldValue);
                await AddActionLogAsync<T>(actionQuery);
                return value;
            }
            else
            {
                return actionQuery.ContentResult;
            }
        }

        public async Task<ActionResult> DeleteOneAsync<T>(long id, [CallerMemberName] string actionName = "") where T : class
        {
            var actionQuery = GetActionQuery<T>(actionName);
            if (actionQuery.IsAllowed)
            {
                var value = context.Set<T>().Find(id);
                SetOldValue(actionQuery, value);
                if (SetObjectIsDeleted(actionQuery, ref value))
                {
                    context.Set<T>().Update(value);
                }
                else
                {
                    context.Set<T>().Remove(value);
                }
                await context.SaveChangesAsync();
                await AddActionLogAsync<T>(actionQuery);
            }
            return actionQuery.ContentResult;
        }


        public async Task<ActionResult<IEnumerable<Models.ObjectType>>> GetObjectTypes([CallerMemberName] string actionName = "")
        {
            //var actionQuery = GetActionQuery<Models.ObjectType>(actionName);

           // if (actionQuery.IsAllowed)
            //{
            var objectTypes = context.ObjectTypes
                .Include(o => o.RootContainer)
                .Include(o => o.Properties)
                .ThenInclude(x => x.EditRoles)
                .ToArray();

            List<Models.ObjectType> resultList = new();

            /*var sapsanAssemblyTypes = typeof(SapsanLib.Models.User).Assembly.GetTypes();
            foreach (var assemblyType in sapsanAssemblyTypes)
            {
                //var baseType = assemblyType.BaseType;
                //while (baseType != null)
                //{
                //if (baseType == typeof(Models.Object))
                //{
                if (assemblyType.Namespace == "SapsanLib.Models")
                {
                    var objectType = context.GetObjectType(assemblyType, null, true);
                    resultList.Add(objectType);
                }
                    //}
                    //baseType = baseType.BaseType;
               // }
            }*/
            var appAssemblyTypes = typeof(Models.Bowling.Bowling).Assembly.GetTypes();
            foreach (var assemblyType in appAssemblyTypes)
            {
                var baseType = assemblyType.BaseType;
                while (baseType != null)
                {
                    if (baseType == typeof(Models.Object))
                    {

                //if (assemblyType.Namespace == "Models.Bowling")
                //{
                    //var overwrite = false;
                    //if (assemblyType.Name == "HolidayOrWeekend" || assemblyType.Name == "CardClock" || assemblyType.Name == "Tabel")
                    //{

                    //    overwrite = true;
                    //}
                        var overwrite = true;
                        var objectType = context.GetObjectType(assemblyType, objectTypes, true, overwrite);
                        if (objectType != null)
                            if (!resultList.Any(o => o.Name == objectType.Name))
                                resultList.Add(objectType);
                //}
                    }
                    baseType = baseType.BaseType;
                }
            }
            //appAssemblyTypes = typeof(Models.User).Assembly.GetTypes();
            //foreach (var assemblyType in appAssemblyTypes)
            //{
            //    //var baseType = assemblyType.BaseType;
            //    //while (baseType != null)
            //    //{
            //    //if (baseType == typeof(Models.Object))
            //    //{

            //    if (assemblyType.Namespace == "PirAppBp.Models")
            //    {

            //        if (assemblyType.Name == "User" || assemblyType.Name == "HolidayOrWeekend")
            //        {
            //            var objectType = context.GetObjectType(assemblyType, null, true, true);
            //            if (objectType != null)
            //                if (!resultList.Any(o => o.Name == objectType.Name))
            //                    resultList.Add(objectType);
            //        }


            //    }
            //    //}
            //    //baseType = baseType.BaseType;
            //    // }
            //}
            //GamesResults.Models.Bowling

            //var objType = context.GetObjectType(typeof(SapsanLib.Models.User), null, true);

            //resultList.Add(objType);

            //}

            // return actionQuery.ContentResult;
            return resultList.ToArray();
        }

        public ActionQuery GetActionQuery<T>(string actionName)
        {
            Models.User? user = null;
            Models.Action? action = null;
            int? logLevel = null;
            string message;
            HttpStatusCode status;

            if (context != null)
            {
                user = context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefault(u => u.Login == UserName);

                if (user != null)
                {
                    action = context.Actions
                        .Include(a => a.Roles)
                        .FirstOrDefault(a => a.Name == actionName);

                    if (action != null)
                    {
                        if (user.Roles.Any(ur => ur.IsAdmin || action.Roles.Any(ar => ar.Name == ur.Name)))
                        {
                            message = "Запрос успешно выполнен";
                            status = HttpStatusCode.OK;
                            if (action.IsChange)
                            {
                                logLevel = 1;
                            }
                        }
                        else
                        {
                            message = "Не достаточно прав доступа";
                            status = HttpStatusCode.Forbidden;
                            logLevel = 2;
                        }
                    }
                    else
                    {
                        message = "Действие не найдено";
                        status = HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    message = "Пользователь не найден";
                    status = HttpStatusCode.Forbidden;
                }
            }
            else
            {
                message = "Контекст базы данных не настроен";
                status = HttpStatusCode.InternalServerError;
            }

            return new ActionQuery(user, action, logLevel, message, status);
        }

        private async Task AddActionLogAsync<T>(ActionQuery actionQuery)
        {
            if (actionQuery.LogLevel != null)
            {
                var obj = actionQuery.Object as Models.Object;
                var log = new Models.Log()
                {
                    Action = actionQuery.Action,
                    User = actionQuery.User,
                    LogLevel = (int)actionQuery.LogLevel,
                    LogText = (obj == null || !actionQuery.IsAllowed) ? actionQuery.Message : null,
                    LogTime = DateTime.Now,
                    ObjectId = obj?.Id
                };
                context.Logs.Add(log);

                if (actionQuery.Action.IsLogDetails)
                {
                    context.LogDetails.Add(new Models.LogDetail()
                    {
                        Log = log,
                        OldValue = actionQuery.OldValue,
                        NewValue = actionQuery.NewValue
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        private bool SetObjectType<T>(ref T value)
        {
            if (value != null)
            {
                if (value is Models.Object obj)
                {
                    var type = typeof(T);
                    obj.Type = context.GetObjectType(type);
                    return true;
                }
            }
            return false;
        }

        private void SetOldValue(ActionQuery actionQuery, object? value)
        {
            actionQuery.SetOldValue(context.Entry(value).OriginalValues.ToObject());
        }

        private void SetNewValue(ActionQuery actionQuery, object? value)
        {
            actionQuery.SetNewValue(context.Entry(value).CurrentValues.ToObject());
        }

        private bool SetObjectAuthor<T>(ActionQuery actionQuery, ref T value)
        {
            var user = actionQuery.User;
            if (value != null && user != null)
            {
                if (value is Models.Object obj)
                {
                    obj.Author = user;
                    obj.AuthorId = user.Id;
                    obj.CreatedAt = DateTime.Now;
                    return true;
                }
            }
            return false;
        }

        private bool SetObjectEditor<T>(ActionQuery actionQuery, ref T value)
        {
            var user = actionQuery.User;
            if (value != null && user != null)
            {
                if (value is Models.Object obj)
                {
                    obj.Editor = user;
                    obj.EditorId = user.Id;
                    obj.ModifiedAt = DateTime.Now;
                    obj.Version++;
                    return true;
                }
            }
            return false;
        }

        private bool SetObjectIsDeleted<T>(ActionQuery actionQuery, ref T value)
        {
            var user = actionQuery.User;
            if (value != null && user != null)
            {
                if (value is Models.Object obj)
                {
                    obj.Deleter = user;
                    obj.DeleterId = user.Id;
                    obj.DeletedAt = DateTime.Now;
                    obj.IsDeleted = true;
                    return true;
                }
            }
            return false;
        }
    }
}
