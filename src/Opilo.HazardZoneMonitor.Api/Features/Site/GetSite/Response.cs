using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;

public sealed record Response(SiteConfiguration Site) : IResponse;
