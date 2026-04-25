namespace Opilo.HazardZoneMonitor.Api.Shared.Database;

public interface ISchemaInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
