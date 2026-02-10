using System.Globalization;
using Microsoft.Data.Sqlite;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Shared.Features;
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

    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=hazardzone.db";

    builder.Services.AddScoped(_ =>
    {
        SqliteConnection connection = new(connectionString);
        connection.Open();
        return connection;
    });
    builder.Services.AddScoped<IMovementsRepository, MovementsRepository>();

    DatabaseInitializer.EnsurePersonMovementsTable(connectionString);

    builder.Services.AddFeaturesFromAssembly(typeof(IApiMarker).Assembly);

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
