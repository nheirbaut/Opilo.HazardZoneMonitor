using Opilo.HazardZoneMonitor.Api.Features.Site.Configuration;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;

public sealed record GetSiteResponse(SiteConfiguration Site) : IResponse;
