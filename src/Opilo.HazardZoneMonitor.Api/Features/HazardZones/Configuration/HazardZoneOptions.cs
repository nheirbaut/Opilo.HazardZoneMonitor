namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;

public sealed record HazardZoneOptions
{
    public IReadOnlyList<HazardZoneConfiguration> HazardZones { get; init; } = [];
}
