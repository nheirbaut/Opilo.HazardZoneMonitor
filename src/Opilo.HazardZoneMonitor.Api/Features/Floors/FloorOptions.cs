namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed record FloorOptions
{
    public IReadOnlyList<FloorConfiguration> Floors { get; init; } = [];
}
