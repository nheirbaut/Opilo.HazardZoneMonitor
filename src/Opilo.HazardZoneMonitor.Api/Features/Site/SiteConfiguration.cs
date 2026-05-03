using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Features.Site;

public sealed record SiteConfiguration(SiteName Name, IReadOnlyList<FloorConfiguration> Floors);
