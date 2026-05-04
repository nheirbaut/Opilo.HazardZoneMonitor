using Opilo.HazardZoneMonitor.Api.Features.Floors;

namespace Opilo.HazardZoneMonitor.Api.Features.Site;

public sealed record SiteConfiguration(string Name, IReadOnlyList<FloorConfiguration> Floors);
