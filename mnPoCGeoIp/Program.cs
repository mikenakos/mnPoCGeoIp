using System.Reflection;
using mnPoCGeoIp;
using mnPoCGeoIp.Extensions;
using mnPoCGeoIp.Services;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Load the essential settings
Appsettings.LoadSettings(builder);

// Add services to the container.
// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c => {

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});


// Add necessary services to the DI container
builder.Services.AddDbContext<mnPoCGeoIp.DataBase.AppDbContext>();
builder.Services.AddScoped<mnPoCGeoIp.DataBase.IIpAddressRepository, mnPoCGeoIp.DataBase.IpAddressRepository>();
builder.Services.AddLookupServices();
builder.Services.AddHostedService<BackgroundProcessingService>();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", Microsoft.Extensions.Logging.LogLevel.None);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
