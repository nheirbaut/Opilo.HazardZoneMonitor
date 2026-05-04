using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Site;

public sealed record SiteOptions
{
    public required SiteName Name { get; init; }
}
