using System.Globalization;
using Opilo.HazardZoneMonitor.Api.Features.FloorManagement;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

Log.Information("Starting HazardZone Monitor API");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.Configure<FloorConfiguration>(builder.Configuration.GetSection("FloorManagement"));
builder.Services.AddSingleton<IFloorRegistry, FloorRegistry>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapGet("/", () => "HazardZone Monitor API");

app.MapGet("/api/v1/floors", (IFloorRegistry registry) => Results.Json(registry.GetAllFloors()));

await app.RunAsync().ConfigureAwait(false);

await Log.CloseAndFlushAsync().ConfigureAwait(false);
