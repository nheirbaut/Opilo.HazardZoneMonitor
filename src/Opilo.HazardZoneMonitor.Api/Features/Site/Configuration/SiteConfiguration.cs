using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;

namespace Opilo.HazardZoneMonitor.Api.Features.Site.Configuration;

public sealed record SiteConfiguration(string Name, IReadOnlyList<FloorConfiguration> Floors);
