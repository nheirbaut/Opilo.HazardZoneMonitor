using System.Globalization;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Features;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Time;
using Scalar.AspNetCore;
using Serilog;

static string ExtractRelFromRoute(string route)
{
    const string prefix = "/api/v1/";
    string rel = route.StartsWith(prefix, StringComparison.Ordinal)
        ? route.Substring(prefix.Length)
        : route;
    return rel;
}

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

    builder.Services
        .AddOptions<HazardZoneOptions>()
        .BindConfiguration(nameof(HazardZoneOptions));

    builder.Services.AddSingleton<IClock, SystemClock>();

    builder.Services.AddOpenApi();

    builder.Services.AddFeaturesFromAssembly(typeof(IApiMarker).Assembly, builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.MapFeaturesFromAssembly(typeof(IApiMarker).Assembly);

    app.MapGet("/", (EndpointDataSource endpointDataSource) => Results.Json(new
    {
        Name = "HazardZone Monitor API",
        Version = "v1",
        Links = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText != null)
            .Where(endpoint => endpoint.RoutePattern.RawText!.StartsWith("/api/v1/", StringComparison.Ordinal))
            .Select(endpoint => new
            {
                Rel = ExtractRelFromRoute(endpoint.RoutePattern.RawText!),
                Href = endpoint.RoutePattern.RawText!,
            })
            .OrderBy(link => link.Rel, StringComparer.Ordinal)
            .ToArray(),
    }));

    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}
