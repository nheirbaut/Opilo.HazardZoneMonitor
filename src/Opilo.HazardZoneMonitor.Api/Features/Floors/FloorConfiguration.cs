using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed record FloorConfiguration
{
    public FloorConfiguration(FloorName name, IReadOnlyList<Coordinate> outline)
    {
        Name = Guard.Against.Null(name);
        Outline = outline;
    }

    public FloorName Name
    {
        get;
        init => field = Guard.Against.Null(value);
    } = null!;

    public IReadOnlyList<Coordinate> Outline { get; init; } = [];

    public IReadOnlyList<HazardZoneConfiguration> HazardZones { get; init; } = [];
}
