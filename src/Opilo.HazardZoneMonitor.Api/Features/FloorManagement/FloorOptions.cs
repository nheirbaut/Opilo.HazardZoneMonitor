namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

public sealed record FloorOptions
{
    public IReadOnlyList<FloorConfiguration> Floors { get; init; } = [];
}
