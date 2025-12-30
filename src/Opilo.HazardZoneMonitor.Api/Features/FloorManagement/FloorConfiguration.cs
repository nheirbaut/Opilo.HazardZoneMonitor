namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

public sealed class FloorConfiguration
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<FloorPointConfiguration> Outline { get; init; } = [];
}
