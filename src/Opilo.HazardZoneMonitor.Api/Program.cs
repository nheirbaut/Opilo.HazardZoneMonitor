using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Features;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Time;
using Scalar.AspNetCore;
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

    builder.Services
        .AddOptions<HazardZoneOptions>()
        .BindConfiguration(nameof(HazardZoneOptions));

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services.AddSingleton<IClock, SystemClock>();
    builder.Services.AddOpenApi(options =>
    {
        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type == typeof(TimeSpan))
            {
                schema.Properties?.Clear();
                schema.Type = JsonSchemaType.String;
                schema.Format = "duration";
            }

            return Task.CompletedTask;
        });
    });
    builder.Services.AddFeaturesFromAssembly(typeof(IApiMarker).Assembly, builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapFeaturesFromAssembly(typeof(IApiMarker).Assembly);

    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}
