using GamesResults;
using GamesResults.Models;
using GamesResults.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GamesResults.Controllers.System
{
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly AppService service;
        private readonly IWebHostEnvironment environment;

        public ImportController(AppService service, IWebHostEnvironment environment)
        {
            this.service = service;
            this.environment = environment;
        }

        [HttpPost("/system/import/file-data")]
        public ActionResult ImportFileData()
        {
            string importFilePath = Path.Combine(environment.ContentRootPath, service.GetConfigSection("ImportData")["ImportFilePath"]);

            var nsContext = service.Context;
            var importer = new FileDataImporter(nsContext);

            importer.ObjectTypesImport(importFilePath, "ObjectTypes");
            importer.DictionaryTypesImport(importFilePath, "DictionaryTypes");
            importer.ObjectPropertiesImport(importFilePath, "ObjectProperties");
            importer.PagesImport(importFilePath, "Pages");
            importer.DictionaryItemsImport(importFilePath, "DictionaryItems");

            return Ok("Импорт справочников из файла успешно завершен");
        }

        //[HttpPost("/system/import/pir-app-data")]
        //public ActionResult ImportPirAppData()
        //{
        //    string pirAppConnectionString = service.GetConfigSection("ImportData")["PirAppConnectionString"];

        //    var pirAppOptionsBuilder = new DbContextOptionsBuilder<PirAppDbContext>();
        //    pirAppOptionsBuilder.UseSqlServer(pirAppConnectionString);

        //    var nsContext = service.Context;
        //    var pirAppContext = new PirAppDbContext(pirAppOptionsBuilder.Options);
        //    var importer = new PirAppDataImporter(nsContext, pirAppContext);

        //    importer.PositionRolesImport();
        //    importer.DepartmentRolesImport();
        //    importer.UsersImport();

        //    return Ok("Импорт пользователей и ролей из АПП-ПИР успешно завершен");
        //}

        [HttpPost("/system/import/pdf-data")]
        public ActionResult ImportPdfData()
        {
           
            return Ok("Импорт данных из pdf успешно завершен");
        }
    }
}
