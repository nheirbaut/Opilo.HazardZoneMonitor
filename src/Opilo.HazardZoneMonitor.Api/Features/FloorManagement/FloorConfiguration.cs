namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

internal sealed class FloorConfiguration
{
    public IReadOnlyList<FloorDto> Floors { get; init; } = [];
}
