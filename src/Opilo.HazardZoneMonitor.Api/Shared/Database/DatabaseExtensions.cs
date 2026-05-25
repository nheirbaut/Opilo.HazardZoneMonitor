namespace Opilo.HazardZoneMonitor.Api.Shared.Database;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseSchemasAsync(this IHost app, CancellationToken cancellationToken = default)
    {
        var initializers = app.Services.GetServices<ISchemaInitializer>();
        foreach (var initializer in initializers)
        {
            await initializer.InitializeAsync(cancellationToken);
        }
    }
}
