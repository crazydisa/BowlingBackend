// See https://aka.ms/new-console-template for more information

using GamesResults;
using GamesResults.Utils;
using GamesResultsUtils;
using Microsoft.EntityFrameworkCore;
//using Npgsql.EntityFrameworkCore;


var nsOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

nsOptionsBuilder.UseNpgsql(Settings.Default.NsConnectionSqlServer);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

//var pirAppOptionsBuilder = new DbContextOptionsBuilder<PirAppDbContext>();
//pirAppOptionsBuilder.UseSqlServer(Settings.Default.PirAppConnectionSqlServer);


//var sapsanOptionsBuilder = new DbContextOptionsBuilder<SapsanDbContext>();
//sapsanOptionsBuilder.UseNpgsql(Settings.Default.SapsanConnectionSqlServer);


Console.Title = "Утилиты для базы данных ИС";

while (true)
{
    Console.Clear();

    var key = MainMenuKey();

    bool isContinue = SelectMainMenuItem(key);

    if (!isContinue) break;

    Console.Clear();
}


ConsoleKey MainMenuKey()
{
    Console.WriteLine("[1] Инициализация БД");
    Console.WriteLine();
    Console.WriteLine("[2] Импорт справочников из файла");
    Console.WriteLine();
    Console.WriteLine("[3] Импорт пользователей и ролей из АПП-ПИР");
    Console.WriteLine();
    Console.WriteLine("[4] Импорт справочников из САПСАН");
    Console.WriteLine();
    Console.WriteLine("Нажмите клавишу, соответствующую разделу меню.");
    Console.WriteLine("Для выхода нажмите любую другую клавишу.");
    Console.WriteLine();

    return Console.ReadKey().Key;
}

bool SelectMainMenuItem(ConsoleKey key)
{
    if (key == ConsoleKey.D1)
    {
        Console.Clear();
        Console.WriteLine("Внимание! БД будет пересоздана, все данные удалены и перезаписаны.");
        Console.WriteLine("Нажмите клавишу [Y] для подтверждения, для отмены нажмите любую другую клавишу.");

        var confirmKey = Console.ReadKey().Key;

        if (confirmKey == ConsoleKey.Y)
        {
            Console.Clear();
            Console.WriteLine("Инициализация БД...");

            var nsContext = new AppDbContext(nsOptionsBuilder.Options);

            var initializer = new NsDbInitializer(nsContext);
            initializer.Initialize();


            return SuccessFinished();
        }
        else return true;
    }

    else if (key == ConsoleKey.D2)
    {
        Console.Clear();
        Console.WriteLine("Импорт справочников из файла");
        Console.WriteLine();

        var nsContext = new AppDbContext(nsOptionsBuilder.Options);
        var importer = new FileDataImporter(nsContext);

        Console.WriteLine("Импорт типов объектов из файла лист {0}...",
            Settings.Default.ObjectTypesSheetName);

        importer.ObjectTypesImport(Settings.Default.ImportFilePath, Settings.Default.ObjectTypesSheetName);


        Console.WriteLine("Импорт типов справочников из файла лист {0}...",
            Settings.Default.DictionaryTypesSheetName);

        importer.DictionaryTypesImport(Settings.Default.ImportFilePath, Settings.Default.DictionaryTypesSheetName);


        Console.WriteLine("Импорт свойств типов объектов из файла лист {0}...",
            Settings.Default.ObjectPropertiesSheetName);

        importer.ObjectPropertiesImport(Settings.Default.ImportFilePath, Settings.Default.ObjectPropertiesSheetName);


        Console.WriteLine("Импорт страниц пользовательского интерфейса из файла лист {0}...",
            Settings.Default.PagesSheetName);

        importer.PagesImport(Settings.Default.ImportFilePath, Settings.Default.PagesSheetName);


        Console.WriteLine("Импорт элементов справочников из файла лист {0}...",
            Settings.Default.DictionaryItemsSheetName);

        importer.DictionaryItemsImport(Settings.Default.ImportFilePath, Settings.Default.DictionaryItemsSheetName);



        return SuccessFinished();
    }

    else if (key == ConsoleKey.D3)
    {
        Console.Clear();
        Console.WriteLine("Импорт пользователей и ролей из АПП-ПИР");
        Console.WriteLine();

        var nsContext = new AppDbContext(nsOptionsBuilder.Options);
        //var pirAppContext = new PirAppDbContext(pirAppOptionsBuilder.Options);
        //var importer = new PirAppDataImporter(nsContext, pirAppContext);

        Console.WriteLine("Импорт должностных ролей...");

        // importer.PositionRolesImport();


        Console.WriteLine("Импорт ролей отделов...");

        // importer.DepartmentRolesImport();


        Console.WriteLine("Импорт пользователей и их ролей...");

        //importer.UsersImport();


        return SuccessFinished();
    }

    else if (key == ConsoleKey.D4)
    {
        Console.Clear();
        Console.WriteLine("Импорт справочников из САПСАН");
        Console.WriteLine();

        var nsContext = new AppDbContext(nsOptionsBuilder.Options);
        //var sapsanContext = new SapsanDbContext(sapsanOptionsBuilder.Options);
        //var importer = new SapsanDataImporter(nsContext, sapsanContext);

        //Console.WriteLine("Импорт должностей...");
        //importer.SprPositionEmployee_Import();

        Console.WriteLine("Импорт Пользователей...");
       // importer.Users_Import();

        //Console.WriteLine("Импорт справочника Организаций...");




        return SuccessFinished();
    }

    else
    {
        return false;
    }
}

bool SuccessFinished()
{
    Console.WriteLine();
    Console.WriteLine("Успешно завершено, нажмите любую клавишу для возврата в меню.");
    Console.ReadKey();

    return true;
}