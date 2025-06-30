using GamesResults.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Principal;
using GamesResults.Interfaces;

namespace GamesResults.Utils
{
    public class FileDataImporter
    {
        private readonly AppDbContext context;
        private readonly User? currentUser;
        private readonly Role? userRole;
        const int StartSortIndex = 16;

        public FileDataImporter(AppDbContext context)
        {
            this.context = context;
            //this.context.ChangeTracker.AutoDetectChangesEnabled = false;
            //this.context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            string currentUserName = WindowsIdentity.GetCurrent().Name;

            currentUser = context.Users.FirstOrDefault(o => o.Name == currentUserName);
            userRole = context.Roles.FirstOrDefault(o => o.Name == "Users");
        }

        public void ObjectTypesImport(string filePath, string sheetName, int headerRowIndex = 1)
        {
            if (filePath != null && sheetName != null)
            {
                var systemType = context.GetObjectType<Models.System>();
                var containerType = context.GetObjectType<Container>();

                FileInfo file = new(filePath);
                if (file.Exists)
                {
                    var xls = new ExcelPackage(file);

                    var sheet = xls.Workbook.Worksheets[sheetName];
                    if (sheet != null)
                    {
                        for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.Dimension.Rows; rowIndex++)
                        {
                            string? name = null;
                            string? title = null;
                            string? names = null;
                            string? titles = null;
                            string? parentContainerName = null;
                            string? containerName = null;
                            string? containerTitle = null;
                            string? containerDescription = null;
                            bool allowGetAction = false;
                            bool allowCreateAction = false;
                            bool allowUpdateAction = false;
                            bool allowDeleteAction = false;
                            bool allowApproveAction = false;
                            bool allowDownloadAction = false;
                            bool allowUploadAction = false;

                            for (int columnIndex = 1; columnIndex <= sheet.Dimension.Columns; columnIndex++)
                            {
                                string headerName = ValueToString(sheet.Cells[headerRowIndex, columnIndex].Value);
                                object cellValue = sheet.Cells[rowIndex, columnIndex].Value;
                                if (cellValue != null)
                                {
                                    switch (headerName)
                                    {
                                        case "Name":
                                            name = ValueToString(cellValue);
                                            break;

                                        case "Title":
                                            title = ValueToString(cellValue);
                                            break;

                                        case "Names":
                                            names = ValueToString(cellValue);
                                            break;

                                        case "Titles":
                                            titles = ValueToString(cellValue);
                                            break;

                                        case "ParentContainerName":
                                            parentContainerName = ValueToString(cellValue);
                                            break;

                                        case "ContainerName":
                                            containerName = ValueToString(cellValue);
                                            break;

                                        case "ContainerTitle":
                                            containerTitle = ValueToString(cellValue);
                                            break;

                                        case "ContainerDescription":
                                            containerDescription = ValueToString(cellValue);
                                            break;

                                        case "AllowGetAction":
                                            allowGetAction = ValueToBoolean(cellValue) ?? false;
                                            break;

                                        case "AllowCreateAction":
                                            allowCreateAction = ValueToBoolean(cellValue) ?? false;
                                            break;

                                        case "AllowUpdateAction":
                                            allowUpdateAction = ValueToBoolean(cellValue) ?? false;
                                            break;

                                        case "AllowDeleteAction":
                                            allowDeleteAction = ValueToBoolean(cellValue) ?? false;
                                            break;

                                        case "AllowApproveAction":
                                            allowApproveAction = ValueToBoolean(cellValue) ?? false;
                                            break;

                                        case "AllowDownloadAction":
                                            allowDownloadAction = ValueToBoolean(cellValue) ?? false;
                                            break;

                                        case "AllowUploadAction":
                                            allowUploadAction = ValueToBoolean(cellValue) ?? false;
                                            break;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(name))
                            {
                                Container? container = null;

                                if (containerName != null)
                                {
                                    if (parentContainerName == null)
                                    {
                                        var system = context.System.FirstOrDefault(o => o.IsSystem && o.Parent == null);

                                        if (system == null)
                                        {
                                            system = new Models.System()
                                            {
                                                //Id = context.GetNextSequenceValue2(),
                                                Type = systemType,
                                                IsSystem = true,
                                                Name = containerName,
                                                Title = containerTitle ?? containerName,
                                                Description = containerDescription,
                                                Author = currentUser,
                                                CreatedAt = DateTime.Now
                                            };
                                            context.System.Add(system);
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            if (system.Title != (containerTitle ?? containerName) ||
                                                system.Description != containerDescription)
                                            {
                                                system.Title = containerTitle ?? containerName;
                                                system.Description = containerDescription;
                                                system.Editor = currentUser;
                                                system.ModifiedAt = DateTime.Now;
                                                system.Version++;
                                            }
                                        }

                                        container = system;
                                    }
                                    else
                                    {
                                        var parentContainer = context.Containers.FirstOrDefault(o => o.Name == parentContainerName);

                                        if (parentContainer != null)
                                        {
                                            container = context.Containers.FirstOrDefault(o => o.Name == containerName);

                                            if (container == null)
                                            {
                                                container = new Models.Container()
                                                {
                                                    //Id = context.GetNextSequenceValue2(),
                                                    Type = containerType,
                                                    Parent = parentContainer,
                                                    Name = containerName,
                                                    Title = containerTitle ?? containerName,
                                                    Description = containerDescription,
                                                    Author = currentUser,
                                                    IsSystem = false,
                                                    CreatedAt = DateTime.Now
                                                };
                                                context.Containers.Add(container);
                                            }
                                            else
                                            {
                                                if (container.Parent != parentContainer ||
                                                    container.Title != (containerTitle ?? containerName) ||
                                                    container.Description != containerDescription)
                                                {
                                                    container.Parent = parentContainer;
                                                    container.Title = containerTitle ?? containerName;
                                                    container.Description = containerDescription;
                                                    container.Editor = currentUser;
                                                    container.ModifiedAt = DateTime.Now;
                                                    container.Version++;
                                                }
                                            }
                                        }
                                        context.SaveChanges();
                                    }
                                }
                                var objectType = context.GetObjectType(name, false); // false - не создавать для типа свойства в БД, будут созданы на последующем этапе
                                if (objectType != null)
                                {
                                    objectType.Title = title ?? name;
                                    objectType.RootContainer = container;
                                    context.SaveChanges();

                                    if (allowGetAction)
                                    {
                                        if (names != null)
                                        {
                                            string getActionName = string.Format("Get{0}", names);
                                            string getActionTitle = string.Format("Получение списка \"{0}\"", titles ?? names);
                                            AddAction(objectType, getActionName, getActionTitle, false, false, true);
                                        }

                                        string getOneActionName = string.Format("Get{0}", name);
                                        string getOneActionTitle = string.Format("Получение \"{0}\"", title);
                                        AddAction(objectType, getOneActionName, getOneActionTitle, false, false, true);
                                        getOneActionName = string.Format("Get{0}ById", name);
                                        getOneActionTitle = string.Format("Получение \"{0}\" по Id", title);
                                        AddAction(objectType, getOneActionName, getOneActionTitle, false, false, true);
                                    }

                                    if (allowCreateAction)
                                    {
                                        string createActionName = string.Format("Create{0}", name);
                                        string createActionTitle = string.Format("Создание \"{0}\"", title);
                                        AddAction(objectType, createActionName, createActionTitle, true, true, false);
                                    }

                                    if (allowUpdateAction)
                                    {
                                        string updateActionName = string.Format("Update{0}", name);
                                        string updateActionTitle = string.Format("Изменение \"{0}\"", title);
                                        AddAction(objectType, updateActionName, updateActionTitle, true, true, false);
                                    }

                                    if (allowDeleteAction)
                                    {
                                        string deleteActionName = string.Format("Delete{0}", name);
                                        string deleteActionTitle = string.Format("Удаление \"{0}\"", title);
                                        AddAction(objectType, deleteActionName, deleteActionTitle, true, true, false);
                                    }

                                    if (allowApproveAction)
                                    {
                                        string approveActionName = string.Format("Approve{0}", name);
                                        string approveActionTitle = string.Format("Утверждение \"{0}\"", title);
                                        AddAction(objectType, approveActionName, approveActionTitle, true, true, false);
                                    }

                                    if (allowDownloadAction)
                                    {
                                        string downloadActionName = string.Format("Download{0}", name);
                                        string downloadActionTitle = string.Format("Скачивание \"{0}\"", title);
                                        AddAction(objectType, downloadActionName, downloadActionTitle, true, false, false);
                                    }

                                    if (allowUploadAction)
                                    {
                                        string uploadActionName = string.Format("Upload{0}", name);
                                        string uploadActionTitle = string.Format("Загрузка на сервер \"{0}\"", title);
                                        AddAction(objectType, uploadActionName, uploadActionTitle, true, false, false);
                                    }

                                    context.SaveChanges();
                                }

                            }
                        }

                        sheet.Dispose();
                    }

                    xls.Dispose();
                }
            }
        }

        private void AddAction(ObjectType objectType,  string actionName, string actionTitle, bool isChange, bool isLogDetails, bool isDefault)
        {
            if (!context.Actions.Any(o => o.Name == actionName))
            {
                var roles = isDefault ? new List<Role> { userRole } : new List<Role>();
                var act = new Models.Action
                    { 
                    ObjectType = objectType,
                    
                    Name = actionName,
                    Title = actionTitle,
                    IsChange = isChange,
                    IsLogDetails = isLogDetails,
                    IsDefault = isDefault,
                    Roles = roles};
                context.Actions.Add(act);
               
            }
        }

        public void DictionaryTypesImport(string filePath, string sheetName, int headerRowIndex = 1)
        {
            if (filePath != null && sheetName != null)
            {
                var dictionaryContainer = context.Containers.FirstOrDefault(o => o.Name == "Dictionaries");
                var dictionatyTypes = context.DictionaryTypes.ToArray();
                var objectTypes = context.ObjectTypes.ToArray();

                FileInfo file = new(filePath);
                if (file.Exists)
                {
                    var xls = new ExcelPackage(file);

                    var sheet = xls.Workbook.Worksheets[sheetName];
                    if (sheet != null)
                    {
                        int sortIndex = 1;
                        for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.Dimension.Rows; rowIndex++)
                        {
                            string? typeName = null;
                            string? name = null;
                            string? title = null;

                            for (int columnIndex = 1; columnIndex <= sheet.Dimension.Columns; columnIndex++)
                            {
                                string headerName = ValueToString(sheet.Cells[headerRowIndex, columnIndex].Value);
                                object cellValue = sheet.Cells[rowIndex, columnIndex].Value;
                                if (cellValue != null)
                                {
                                    switch (headerName)
                                    {
                                        case "TypeName":
                                            typeName = ValueToString(cellValue);
                                            break;

                                        case "Name":
                                            name = ValueToString(cellValue);
                                            break;

                                        case "Title":
                                            title = ValueToString(cellValue);
                                            break;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(title))
                            {
                                var objectType = objectTypes.FirstOrDefault(o => o.Name == typeName);
                                if (objectType != null)
                                {
                                    var dictionatyType = dictionatyTypes.FirstOrDefault(o => o.Name == name);
                                    if (dictionatyType == null)
                                    {
                                        dictionatyType = new DictionaryType()
                                        {
                                            //Id = context.GetNextSequenceValue2(),
                                            SortIndex = sortIndex,
                                            Type = objectType,
                                            Parent = dictionaryContainer,
                                            Name = name,
                                            Title = title,
                                            Author = currentUser,
                                            CreatedAt = DateTime.Now
                                        };
                                        context.DictionaryTypes.Add(dictionatyType);
                                    }
                                    else
                                    {
                                        if (dictionatyType.SortIndex != sortIndex ||
                                            dictionatyType.Title != title)
                                        {
                                            dictionatyType.SortIndex = sortIndex;
                                            dictionatyType.Title = title;
                                            dictionatyType.Editor = currentUser;
                                            dictionatyType.ModifiedAt = DateTime.Now;
                                            dictionatyType.Version++;
                                        }
                                    }

                                    if (objectType.Name != "DictionaryItem")
                                    {
                                        objectType.RootContainer = dictionatyType;
                                    }

                                    context.SaveChanges();
                                    sortIndex++;
                                }
                            }
                        }

                        sheet.Dispose();
                    }

                    xls.Dispose();
                }
            }
        }
        public void ObjectPropertiesImport(string filePath, string sheetName, int headerRowIndex = 1)
        {
            if (filePath != null && sheetName != null)
            {
                FileInfo file = new(filePath);
                if (file.Exists)
                {
                    var xls = new ExcelPackage(file);

                    var sheet = xls.Workbook.Worksheets[sheetName];
                    if (sheet != null)
                    {
                        string? objectTypeName = null;
                        ObjectType? objectType = null;
                        int sortIndex = StartSortIndex;
                        for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.Dimension.Rows; rowIndex++)
                        {
                            string? propertyTypeName = null;
                            string? propertyName = null;
                            string? propertyTitle = null;
                            string? propertyGroupTitle = null;
                            string? propertySubGroupTitle = null;
                            string? propertyDescription = null;
                            string? propertyDataFormat = null;
                            string? propertyDisplayExpr = null;

                            string? propertyRelatedField = null;
                            string? propertyRelatedType = null;

                            bool? propertyIsInclude = null;
                            bool? propertyIsHiddenByDefault = null;
                            bool? propertyIsReadOnly = null;
                            bool? propertyIsMultiline = null;
                            string[]? propertyEditRoleTitles = null;

                            for (int columnIndex = 1; columnIndex <= sheet.Dimension.Columns; columnIndex++)
                            {
                                string? headerName = ValueToString(sheet.Cells[headerRowIndex, columnIndex].Value);
                                object cellValue = sheet.Cells[rowIndex, columnIndex].Value;
                                if (cellValue != null)
                                {
                                    switch (headerName)
                                    {
                                        case "TypeName":
                                            propertyTypeName = ValueToString(cellValue);
                                            break;

                                        case "Name":
                                            propertyName = ValueToString(cellValue);
                                            break;

                                        case "Title":
                                            propertyTitle = ValueToString(cellValue);
                                            break;

                                        case "GroupTitle":
                                            propertyGroupTitle = ValueToString(cellValue);
                                            break;

                                        case "SubGroupTitle":
                                            propertySubGroupTitle = ValueToString(cellValue);
                                            break;

                                        case "Description":
                                            propertyDescription = ValueToString(cellValue);
                                            break;

                                        case "DataFormat":
                                            propertyDataFormat = ValueToString(cellValue);
                                            break;

                                        case "DisplayExpr":
                                            propertyDisplayExpr = ValueToString(cellValue);
                                            break;

                                        case "IsInclude":
                                            propertyIsInclude = ValueToBoolean(cellValue);
                                            break;

                                        case "IsHiddenByDefault":
                                            propertyIsHiddenByDefault = ValueToBoolean(cellValue);
                                            break;

                                        case "IsReadOnly":
                                            propertyIsReadOnly = ValueToBoolean(cellValue);
                                            break;

                                        case "IsMultiline":
                                            propertyIsMultiline = ValueToBoolean(cellValue);
                                            break;

                                        case "EditRoles":
                                            propertyEditRoleTitles = ValueToStringArray(cellValue);
                                            break;

                                        case "RelatedField":
                                            propertyRelatedField = ValueToString(cellValue);
                                            break;

                                        case "RelatedType":
                                            propertyRelatedType = ValueToString(cellValue);
                                            break;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(propertyTypeName))
                            {
                                if (objectTypeName != propertyTypeName)
                                {
                                    objectType = context.GetObjectType(propertyTypeName);
                                    objectTypeName = propertyTypeName;
                                    sortIndex = StartSortIndex;
                                }

                                if (objectType != null && !string.IsNullOrEmpty(propertyName))
                                {
                                    List<Role>? editRoles = null;

                                    if (propertyEditRoleTitles != null)
                                    {
                                        foreach (string roleTitle in propertyEditRoleTitles)
                                        {
                                            var role = context.Roles.FirstOrDefault(o => o.Title == roleTitle);
                                            if (role == null)
                                            {
                                                role = new Role()
                                                {
                                                    Name = roleTitle,
                                                    Title = roleTitle
                                                };
                                                context.Roles.Add(role);
                                            }
                                            if (editRoles == null)
                                            {
                                                editRoles = new List<Role>();
                                            }
                                            editRoles.Add(role);
                                        }
                                    }
                                    ObjectProperty? idProperty=null;
                                    
                                    if (propertyRelatedField != null)
                                        idProperty = objectType?.Properties?.FirstOrDefault(o => o.Name == propertyRelatedField);
                                    if (idProperty == null)
                                        idProperty = objectType?.Properties?.FirstOrDefault(o => o.Name == propertyName + "Id");
                                    if (idProperty == null && !propertyName.EndsWith("Navigation"))
                                        idProperty = objectType?.Properties?.FirstOrDefault(o => o.Name == "Id" + propertyName);
                                    if (idProperty != null)
                                    {
                                        if (idProperty.IsIdentifier)
                                        {
                                            idProperty.SortIndex = sortIndex;
                                            idProperty.Title = string.Format("Id {0}", propertyTitle ?? propertyName);
                                            idProperty.GroupTitle = propertyGroupTitle;
                                            idProperty.SubGroupTitle = propertySubGroupTitle;
                                            idProperty.Description = propertyDescription;
                                            if (editRoles != null)
                                            {
                                                context.ObjectProperties
                                                    .Include(p => p.EditRoles)
                                                    .FirstOrDefault(p => p.Id == idProperty.Id);

                                                idProperty.EditRoles = editRoles;
                                            }
                                            if (propertyDataFormat != null)
                                            {
                                                idProperty.DataFormat = propertyDataFormat;
                                            }
                                            if (propertyDisplayExpr != null)
                                            {
                                                idProperty.DisplayExpr = propertyDisplayExpr;
                                            }
                                            if (propertyIsInclude != null)
                                            {
                                                idProperty.IsInclude = propertyIsInclude.Value;
                                            }
                                            if (propertyIsHiddenByDefault != null)
                                            {
                                                idProperty.IsHiddenByDefault = propertyIsHiddenByDefault.Value;
                                            }
                                            if (propertyIsReadOnly != null)
                                            {
                                                idProperty.IsReadOnly = propertyIsReadOnly.Value;
                                            }
                                            if (propertyIsMultiline != null)
                                            {
                                                idProperty.IsMultiline = propertyIsMultiline.Value;
                                            }
                                            
                                        }
                                    }

                                    var property = objectType?.Properties?.FirstOrDefault(o => o.Name == propertyName);
                                    if (property != null)
                                    {
                                        property.SortIndex = sortIndex;
                                        property.Title = propertyTitle ?? propertyName;
                                        property.GroupTitle = propertyGroupTitle;
                                        property.SubGroupTitle = propertySubGroupTitle;
                                        property.Description = propertyDescription;
                                        if (editRoles != null)
                                        {
                                            context.ObjectProperties
                                                .Include(p => p.EditRoles)
                                                .FirstOrDefault(p => p.Id == property.Id);

                                            property.EditRoles = editRoles;
                                        }
                                        if (propertyDataFormat != null)
                                        {
                                            property.DataFormat = propertyDataFormat;
                                        }
                                        if (propertyDisplayExpr != null)
                                        {
                                            property.DisplayExpr = propertyDisplayExpr;
                                        }
                                        if (propertyIsInclude != null)
                                        {
                                            property.IsInclude = propertyIsInclude.Value;
                                        }
                                        if (propertyIsHiddenByDefault != null)
                                        {
                                            property.IsHiddenByDefault = propertyIsHiddenByDefault.Value;
                                        }
                                        if (propertyIsReadOnly != null)
                                        {
                                            property.IsReadOnly = propertyIsReadOnly.Value;
                                        }
                                        if (propertyIsMultiline != null)
                                        {
                                            property.IsMultiline = propertyIsMultiline.Value;
                                        }
                                        if (propertyRelatedField != null)
                                        {
                                            property.RelatedField = propertyRelatedField;
                                        }
                                        if (propertyRelatedType != null)
                                        {
                                            property.RelatedType = propertyRelatedType;
                                            property.TypeName = propertyRelatedType;
                                        }
                                    }

                                    context.SaveChanges();

                                    sortIndex++;
                                }
                            }
                        }

                        sheet.Dispose();
                    }

                    xls.Dispose();
                }
            }
        }

        public void PagesImport(string filePath, string sheetName, int headerRowIndex = 1)
        {
            if (filePath != null && sheetName != null)
            {
                var pages = context.Pages.ToArray();
                var objectType = context.GetObjectType<Page>();

                FileInfo file = new(filePath);
                if (file.Exists)
                {
                    var xls = new ExcelPackage(file);

                    var sheet = xls.Workbook.Worksheets[sheetName];
                    if (sheet != null)
                    {
                        int sortIndex = 1;
                        for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.Dimension.Rows; rowIndex++)
                        {
                            string? path = null;
                            string? name = null;
                            string? containerName = null;
                            string? title = null;
                            string? description = null;

                            for (int columnIndex = 1; columnIndex <= sheet.Dimension.Columns; columnIndex++)
                            {
                                string headerName = ValueToString(sheet.Cells[headerRowIndex, columnIndex].Value);
                                object cellValue = sheet.Cells[rowIndex, columnIndex].Value;
                                if (cellValue != null)
                                {
                                    switch (headerName)
                                    {
                                        case "Path":
                                            path = ValueToString(cellValue);
                                            break;

                                        case "Name":
                                            name = ValueToString(cellValue);
                                            break;

                                        case "ContainerName":
                                            containerName = ValueToString(cellValue);
                                            break;
                                        case "Title":
                                            title = ValueToString(cellValue);
                                            break;

                                        case "Description":
                                            description = ValueToString(cellValue);
                                            break;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(title))
                            {
                                Container? pageContainer;
                                if (!string.IsNullOrEmpty(containerName))
                                {
                                    pageContainer = context.Containers.FirstOrDefault(o => o.Name == containerName);
                                }
                                else
                                {
                                    pageContainer = context.Containers.FirstOrDefault(o => o.Name == "Pages");
                                }

                                var page = pages.FirstOrDefault(o => o.Name == name);

                                if (page == null)
                                {
                                    var pageLoadAction = new Models.Action()
                                    {
                                        Name = string.Format("Load{0}", name),
                                        Title = string.Format("Загрузка страницы \"{0}\"", title),
                                        ObjectType = objectType,
                                        IsDefault = true,
                                        Roles = new List<Role> { userRole }
                                    };

                                    context.Actions.Add(pageLoadAction);

                                    context.Pages.Add(new Page()
                                    {
                                        //Id = context.GetNextSequenceValue2(),
                                        SortIndex = sortIndex,
                                        Type = objectType,
                                        Parent = pageContainer,
                                        Path = path,
                                        Name = name,
                                        Title = title,
                                        Description = description,
                                        LoadAction = pageLoadAction,
                                        Author = currentUser,
                                        CreatedAt = DateTime.Now
                                    });
                                    context.SaveChanges();
                                }
                                else
                                {
                                    if (page.SortIndex != sortIndex ||
                                        page.Parent != pageContainer ||
                                        page.Path != path ||
                                        page.Title != title ||
                                        page.Description != description)
                                    {
                                        page.SortIndex = sortIndex;
                                        page.Parent = pageContainer;
                                        page.Path = path;
                                        page.Title = title;
                                        page.Description = description;
                                        page.Editor = currentUser;
                                        page.ModifiedAt = DateTime.Now;
                                        page.Version++;
                                        context.SaveChanges();
                                    }
                                }

                                sortIndex++;
                            }
                        }

                        sheet.Dispose();
                    }

                    xls.Dispose();
                }
            }
        }

        public void DictionaryItemsImport(string filePath, string sheetName, int headerRowIndex = 1)
        {
            if (filePath != null && sheetName != null)
            {
                var dictionatyTypes = context.DictionaryTypes.ToArray();
                var dictionatyItems = context.DictionaryItems.ToArray();
                var objectType = context.GetObjectType<DictionaryItem>();

                FileInfo file = new(filePath);
                if (file.Exists)
                {
                    var xls = new ExcelPackage(file);

                    var sheet = xls.Workbook.Worksheets[sheetName];
                    if (sheet != null)
                    {
                        for (int rowIndex = headerRowIndex + 1; rowIndex <= sheet.Dimension.Rows; rowIndex++)
                        {
                            DictionaryType? dictionaryType = null;
                            string? name = null;
                            string? title = null;
                            string? description = null;
                            bool isNotUsed = false;

                            for (int columnIndex = 1; columnIndex <= sheet.Dimension.Columns; columnIndex++)
                            {
                                string headerName = ValueToString(sheet.Cells[headerRowIndex, columnIndex].Value);
                                object cellValue = sheet.Cells[rowIndex, columnIndex].Value;
                                if (cellValue != null)
                                {
                                    switch (headerName)
                                    {
                                        case "TypeName":
                                            dictionaryType = ValueToDictionaryType(cellValue, dictionatyTypes);
                                            break;

                                        case "Name":
                                            name = ValueToString(cellValue);
                                            break;

                                        case "Title":
                                            title = ValueToString(cellValue);
                                            break;

                                        case "Description":
                                            description = ValueToString(cellValue);
                                            break;

                                        case "IsNotUsed":
                                            isNotUsed = cellValue != null;
                                            break;
                                    }
                                }
                            }

                            if (dictionaryType != null && !string.IsNullOrEmpty(title))
                            {
                                var dictionaryItem = dictionatyItems.FirstOrDefault(o => o.DictionaryTypeId == dictionaryType.Id && o.Title == title);
                                if (dictionaryItem == null)
                                {
                                    context.DictionaryItems.Add(new DictionaryItem()
                                    {
                                        SortIndex = context.DictionaryItems.Count(o => o.DictionaryTypeId == dictionaryType.Id) + 1,
                                        Type = objectType,
                                        Parent = dictionaryType,
                                        DictionaryType = dictionaryType,
                                        Name = name,
                                        Title = title,
                                        Description = description,
                                        IsNotUsed = isNotUsed,
                                        Author = currentUser,
                                        CreatedAt = DateTime.Now
                                    });
                                    context.SaveChanges();
                                }
                                else
                                {
                                    if (dictionaryItem.Description != description || dictionaryItem.IsNotUsed != isNotUsed)
                                    {
                                        dictionaryItem.Description = description;
                                        dictionaryItem.IsNotUsed = isNotUsed;
                                        dictionaryItem.Editor = currentUser;
                                        dictionaryItem.ModifiedAt = DateTime.Now;
                                        dictionaryItem.Version++;
                                        context.SaveChanges();
                                    }
                                }
                            }
                        }

                        sheet.Dispose();
                    }

                    xls.Dispose();
                }
            }
        }


        private static string? ValueToString(object value)
        {
            return value?.ToString()?.Trim();
        }

        private static string[]? ValueToStringArray(object value)
        {
            return value?.ToString()?.Split(",;\r\n".ToArray(), StringSplitOptions.TrimEntries);
        }

        private static float? ValueToFloat(object value)
        {
            if (float.TryParse(value?.ToString(), out float result))
            {
                return result;
            }
            return null;
        }

        private static bool? ValueToBoolean(object value)
        {
            if (value != null)
            {
                string strValue = value.ToString().Trim().ToLower();
                if (strValue == "нет" || strValue == "ложь" || strValue == "-" || strValue == "0") return false;
                else if (strValue == "да" || strValue == "истина" || strValue == "+" || strValue == "1") return true;
                if (bool.TryParse(value?.ToString(), out bool result))
                {
                    return result;
                }
            }
            return null;
        }

        private static DictionaryType? ValueToDictionaryType(object value, DictionaryType[] dictionaryTypes)
        {
            string typeName = ValueToString(value);
            if (typeName != null)
            {
                return dictionaryTypes.FirstOrDefault(type => type.Name == typeName);
            }
            return null;
        }

        private static DictionaryItem? ValueToDictionaryItem(object value, string dictionatyTypeName, DictionaryType[] dictionaryTypes, DictionaryItem[] dictionaryItems)
        {
            string itemTitle = ValueToString(value);
            if (itemTitle != null)
            {
                var dictionaryType = dictionaryTypes.FirstOrDefault(type => type.Name == dictionatyTypeName);
                if (dictionaryType != null)
                {
                    return dictionaryItems.FirstOrDefault(item => item.DictionaryTypeId == dictionaryType.Id && item.Title == itemTitle);
                }
            }
            return null;
        }

        //private static PirAppLib.Models.App1.Department? ValueToDepartment(object value, PirAppLib.Models.App1.Department[] departments)
        //{
        //    string departmentTitle = ValueToString(value);
        //    if (departmentTitle != null)
        //    {
        //        return departments.FirstOrDefault(department => department.Title == departmentTitle);
        //    }
        //    return null;
        //}

        private static int? ValueToInt(object value)
        {
            if (int.TryParse(value?.ToString(), out int result))
            {
                return result;
            }
            return null;
        }

        private static float? ValueToLong(object value)
        {
            if (long.TryParse(value?.ToString(), out long result))
            {
                return result;
            }
            return null;
        }

        private static DateTime? ValueToDateTime(object value)
        {
            if (DateTime.TryParse(value?.ToString(), out DateTime result))
            {
                return result;
            }
            return null;
        }

    }
}
