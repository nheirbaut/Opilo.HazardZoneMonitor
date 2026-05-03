using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed record FloorConfiguration(
    FloorName Name,
    IReadOnlyList<PointConfiguration> Outline)
{
    public IReadOnlyList<HazardZoneConfiguration> HazardZones { get; init; } = [];
}
