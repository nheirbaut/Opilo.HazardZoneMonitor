using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed record FloorConfiguration(
    string Name,
    IReadOnlyList<Coordinate> Outline)
{
    public IReadOnlyList<HazardZoneConfiguration> HazardZones { get; init; } = [];
}
