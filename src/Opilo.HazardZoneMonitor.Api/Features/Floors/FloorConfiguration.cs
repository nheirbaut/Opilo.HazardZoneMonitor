using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed record FloorConfiguration(
    string Name,
    IReadOnlyList<PointConfiguration> Outline)
{
    public IReadOnlyList<HazardZoneConfiguration> HazardZones { get; init; } = [];
}
