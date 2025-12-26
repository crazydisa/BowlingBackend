using GamesResults;
using GamesResults.Interfaces;
using GamesResults.Models;
using GamesResults.Utils;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
//var pgHost = Environment.GetEnvironmentVariable("Postgres.PGHOST");
//var pgPort = Environment.GetEnvironmentVariable("Postgres.PGPORT");
//var pgDataBase = Environment.GetEnvironmentVariable("Postgres.PGDATABASE");
//var pgUser = Environment.GetEnvironmentVariable("Postgres.PGUSER");
//var pgPassword = Environment.GetEnvironmentVariable("Postgres.PGPASSWORD");
//var conStr = $"Server={pgHost};Port={pgPort};Database={pgDataBase};UserId={pgUser};Password={pgPassword}";
//var conStr = $"Server=postgres.railway.internal;Port=5432;Database=railway;UserId=postgres;Password=TzyKfOwtBFcGoUTxzOESTiljuydGCQyc";
var conStr = $"Server=localhost;Port=5432;Database=bowling;UserId=postgres;Password=postgres";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",policy => //AddDefaultPolicy
    {
        //policy.AllowAnyOrigin()
        //        .AllowAnyHeader()
        //        .AllowAnyMethod();
        policy.WithOrigins("http://localhost:8080");  // Разрешённый источник
        policy.AllowAnyMethod();                           // Любые методы (GET, POST и т. д.)
        policy.AllowAnyHeader();                         // Любые заголовки
            policy.AllowCredentials();

    });
   
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.PropertyNamingPolicy = null);
//var connectionStringSapsan = builder.Configuration.GetConnectionString("SapsanConnectionString");
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DbContext, AppDbContext>(options => options.UseNpgsql(conStr));



// Регистрируем фоновый сервис
builder.Services.AddHostedService<RatingUpdateService>();

builder.Services.AddScoped<AppService>();
// Регистрация сервисов
builder.Services.AddScoped<IRatingService, EloRatingService>();

// Настройка авторизации (если еще не добавлено)
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy =>
//        policy.RequireRole("Admin", "SuperAdmin"));

//    options.AddPolicy("OrganizerAccess", policy =>
//        policy.RequireRole("Admin", "SuperAdmin", "Organizer"));
//});

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Веб-сервис API", Version = "v1" });
    c.CustomSchemaIds(x => x.FullName);
});

//builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

//if (builder.Environment.IsDevelopment())
//{
//    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//    {
//        builder.WebHost.UseHttpSys(options =>
//        {
//            options.Authentication.AllowAnonymous = true; // иначе не работает axios cors POST, PUT
//            options.Authentication.Schemes =
//                    AuthenticationSchemes.Negotiate |
//                    AuthenticationSchemes.NTLM;
//        });
//    }
//}

//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = options.DefaultPolicy;
//});

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;

//    //NsDbInitializer.Initialize(services);
//}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    Console.WriteLine("Connection string!!: " + config.GetConnectionString("DefaultConnection"));
    Console.WriteLine("Connection string2: " + "Server=${PGHOST};Port=${PGPORT};Database=${PGDATABASE};UserId=${PGUSER};Password=${PGPASSWORD};");
    Console.WriteLine("Connection string3: " + "Server=${{PGHOST}};Port=${{PGPORT}};Database=${{PGDATABASE}};UserId=${{PGUSER}};Password=${{PGPASSWORD}}");
    Console.WriteLine("Connection string4: " + conStr);
    try
    {
        
        
        // 1. Применяем миграции
        db.Database.Migrate();

        // 2. Заполняем справочники (ваш код из консольного проекта)
        //var nsOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        //nsOptionsBuilder.UseNpgsql(connectionString);

        //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        //var nsContext = new AppDbContext(nsOptionsBuilder.Options);

        //var initializer = new NsDbInitializer(nsContext);
        //initializer.Initialize();
        //var dataImporter = services.GetRequiredService<DataImporter>();
        //await dataImporter.SeedFromExcelAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка при инициализации БД");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowSpecificOrigin"); //убрать, только локально
app.UseHttpsRedirection();
app.UseMiddleware<AppErrorHandler>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().AllowAnonymous();
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),"Uploads")),
//    RequestPath = "/uploads"
//});

app.Run();
