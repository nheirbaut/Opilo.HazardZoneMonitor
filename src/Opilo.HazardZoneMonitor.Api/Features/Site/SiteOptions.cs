using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Features.Site;

public sealed record SiteOptions
{
    public SiteName Name { get; init; }
}
