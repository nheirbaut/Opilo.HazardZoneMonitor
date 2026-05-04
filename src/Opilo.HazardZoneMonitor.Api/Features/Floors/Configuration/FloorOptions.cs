namespace Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;

public sealed record FloorOptions
{
    public IReadOnlyList<FloorConfiguration> Floors { get; init; } = [];
}
