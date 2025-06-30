using GamesResults;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
options.AddDefaultPolicy(
    policy =>
    {
        policy.WithOrigins("http://localhost:8081") //http://localhost:8080
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();


    });
    //options.AddPolicy("CorsPolicy",
    //            builder => builder
    //                .AllowAnyMethod()
    //                .AllowCredentials()
    //                .SetIsOriginAllowed((host) => { return host == "file://"; })
    //                .AllowAnyHeader());
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.PropertyNamingPolicy = null);
var connectionStringSapsan = builder.Configuration.GetConnectionString("SapsanConnectionString");
var connectionString = builder.Configuration.GetConnectionString("ConnectionSqlServer");
builder.Services.AddDbContext<DbContext, AppDbContext>(options => options.UseNpgsql(connectionString));
//builder.Services.AddDbContext<DbContext, SapsanLib.SapsanDbContext>(options => options.UseNpgsql(connectionStringSapsan));




builder.Services.AddScoped<AppService>();
//builder.Services.AddScoped<SapsanLib.SapsanDbContext>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Веб-сервис API", Version = "v1" });
    c.CustomSchemaIds(x => x.FullName);
});

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

if (builder.Environment.IsDevelopment())
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        builder.WebHost.UseHttpSys(options =>
        {
            options.Authentication.AllowAnonymous = true; // иначе не работает axios cors POST, PUT
            options.Authentication.Schemes =
                    AuthenticationSchemes.Negotiate |
                    AuthenticationSchemes.NTLM;
        });
    }
}

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    //NsDbInitializer.Initialize(services);
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

app.UseCors();
//app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseMiddleware<AppErrorHandler>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(),"Uploads")),
    RequestPath = "/uploads"
});

app.Run();
