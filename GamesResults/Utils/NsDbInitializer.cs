using GamesResults.Models;

namespace GamesResults.Utils
{
    public class NsDbInitializer
    {
        private readonly AppDbContext context;

        public NsDbInitializer(AppDbContext context)
        {
            this.context = context;
        }

        public void Initialize()
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

             if (!context.Users.Any())
            {
                // Инициализация группы пользователей

                var userRole = new Role()
                {
                    Name = "Users",
                    Title = "Пользователи"
                };
                context.Roles.Add(userRole);
                context.SaveChanges();


                // Инициализация администраторов

                var adminRole = new Role()
                {
                    Name = "Admins",
                    Title = "Администраторы",
                    IsAdmin = true,
                };
                context.Roles.Add(adminRole);

                context.Users.Add(new User()
                {
                    Name = "ROSNEFT\\mvsukharev",
                    Title = "Сухарев М.В.",
                    EMail = "mvsukharev@tnnc.rosneft.ru",
                    Roles = new List<Role> { adminRole }
                });
                context.Users.Add(new User()
                {
                    Name = "ROSNEFT\\dbbarulin",
                    Login = "ROSNEFT\\dbbarulin",
                    Title = "Барулин Д.Б.",
                    EMail = "dbbarulin@tnnc.rosneft.ru",
                    Roles = new List<Role> { adminRole }
                });
                context.Users.Add(new User()
                {
                    Name = "ROSNEFT\\avdanilenko",
                    Title = "Даниленко А.В.",
                    EMail = "avdanilenko@tnnc.rosneft.ru",
                    Roles = new List<Role> { adminRole }
                });

                context.SaveChanges();


                // Инициализация базовых типов

                var systemType = context.GetObjectType<Models.System>();
                systemType.Title = "Система";

                var containerType = context.GetObjectType<Models.Container>();
                containerType.Title = "Контейнер";

                context.SaveChanges();


                // Инициализация базовых действий

                context.Actions.Add(new Models.Action()
                {
                    Name = "GetObjectTypes",
                    Title = "Получить типы объектов",
                    IsDefault = true,
                    Roles = new List<Role> { userRole }
                });
                context.Actions.Add(new Models.Action()
                {
                    Name = "GetObjects",
                    Title = "Получить объекты",
                    IsDefault = true,
                    Roles = new List<Role> { userRole }
                });
                context.Actions.Add(new Models.Action()
                {
                    Name = "GetUsers",
                    Title = "Получить пользователей",
                    IsDefault = true,
                    Roles = new List<Role> { userRole }
                });
                context.Actions.Add(new Models.Action()
                {
                    Name = "GetLogs",
                    Title = "Получить журнал изменений",
                    IsDefault = true,
                    Roles = new List<Role> { userRole }
                });
                context.Actions.Add(new Models.Action()
                {
                    Name = "GetLogDetails",
                    Title = "Получить детали журнала изменений",
                    IsDefault = true,
                    Roles = new List<Role> { userRole }
                });

                context.Actions.Add(new Models.Action()
                {
                    Name = "GetRoles",
                    Title = "Получить роли",
                });
                context.Actions.Add(new Models.Action()
                {
                    Name = "UpdateActionRoles",
                    Title = "Изменить роли для действия",
                    IsChange = true
                });
                //var dictionaryContainer = context.Containers.FirstOrDefault(o => o.Name == "Dictionaries");
                //var projectType = context.GetObjectType<Models.Project>();
                //var complexType = context.GetObjectType<Models.Complex>();
                //context.DictionaryTypes.Add(new DictionaryType()
                //{
                //    Name = "Projects",
                //    Title = "Projects",
                //    Parent = dictionaryContainer,
                //    Type = projectType,
                //    Author = context.Users.FirstOrDefault()
                //});
                //context.DictionaryTypes.Add(new DictionaryType()
                //{
                //    Name = "Complexes",
                //    Title = "Complexes",
                //    Parent = dictionaryContainer,
                //    Type = complexType,
                //    Author = context.Users.FirstOrDefault()
                //});
                //context.SaveChanges();
                //var projects = context.DictionaryTypes.FirstOrDefault(o => o.Name == "Projects");
                //var complexes = context.DictionaryTypes.FirstOrDefault(o => o.Name == "Complexes");
               
                //projectType.RootContainer = projects;
                //complexType.RootContainer = complexes;
                
                context.SaveChanges();
            }
        }
    }
}
