using System.Globalization;
using Opilo.HazardZoneMonitor.Api.Features.FloorManagement;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Only use bootstrap logger in non-development environments to avoid "logger already frozen" errors
if (!builder.Environment.IsEnvironment("Development"))
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .CreateBootstrapLogger();

    Log.Information("Starting HazardZone Monitor API");
}

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.Configure<FloorConfiguration>(builder.Configuration.GetSection("FloorManagement"));
builder.Services.AddSingleton<IFloorRegistry, FloorRegistry>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapGet("/", () => "HazardZone Monitor API");
app.MapGet("/api/v1/floors", () => new GetFloorResponse([]));

await app.RunAsync().ConfigureAwait(false);

await Log.CloseAndFlushAsync().ConfigureAwait(false);
