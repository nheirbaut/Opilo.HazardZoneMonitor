using System.Globalization;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Shared.Features;
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

builder.Services.AddFeaturesFromAssembly(typeof(IApiMarker).Assembly);

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapGet("/", () => "HazardZone Monitor API");
app.MapPost("/api/v1/person-movements", (RegisterPersonMovementRequest request)
    => TypedResults.Created(new Uri("/api/v1/person-movements/1", UriKind.Relative), new RegisteredPersonMovementDto(request.PersonId)));

app.MapFeaturesFromAssembly(typeof(IApiMarker).Assembly);

await app.RunAsync().ConfigureAwait(false);

await Log.CloseAndFlushAsync().ConfigureAwait(false);
