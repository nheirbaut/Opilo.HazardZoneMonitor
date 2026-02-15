namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed record HazardZoneOptions
{
    public IReadOnlyList<HazardZoneConfiguration> HazardZones { get; init; } = [];
}
