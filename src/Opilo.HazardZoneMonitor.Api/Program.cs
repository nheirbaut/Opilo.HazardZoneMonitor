using System.Globalization;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Shared.Features;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Time;
using Serilog;

try
{
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

    builder.Services.AddSingleton<IClock, SystemClock>();

    builder.Services.AddFeaturesFromAssembly(typeof(IApiMarker).Assembly, builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapGet("/", () => "HazardZone Monitor API");
    app.MapFeaturesFromAssembly(typeof(IApiMarker).Assembly);

    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}
