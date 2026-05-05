namespace Opilo.HazardZoneMonitor.Api.Shared.Database;

public static class DatabaseExtensions
{
    public static void InitializeDatabaseSchemas(this IHost app)
    {
        var initializers = app.Services.GetServices<ISchemaInitializer>();
        foreach (var initializer in initializers)
        {
            initializer.InitializeAsync().GetAwaiter().GetResult();
        }
    }
}
