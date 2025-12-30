using System.Globalization;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.FloorManagement;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking;
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

builder.Services
    .AddOptions<FloorOptions>()
    .BindConfiguration(nameof(FloorOptions));

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapGet("/", () => "HazardZone Monitor API");
app.MapGet("/api/v1/floors", (IOptions<FloorOptions> floorConfiguration)
    => new GetFloorResponse(floorConfiguration.Value.Floors));
app.MapPost("/api/v1/person-movements", (RegisterPersonMovementRequest request)
    => Results.Created(new Uri("/api/v1/person-movements/1", UriKind.Relative), null));

await app.RunAsync().ConfigureAwait(false);

await Log.CloseAndFlushAsync().ConfigureAwait(false);
