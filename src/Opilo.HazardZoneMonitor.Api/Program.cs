using System.Globalization;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

Log.Information("Starting HazardZone Monitor API");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapGet("/", () => "HazardZone Monitor API");

app.MapGet("/api/v1/floors", () => "[]");

await app.RunAsync().ConfigureAwait(false);

await Log.CloseAndFlushAsync().ConfigureAwait(false);
