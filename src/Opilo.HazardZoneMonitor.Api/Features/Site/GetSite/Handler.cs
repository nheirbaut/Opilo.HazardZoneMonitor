using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;

public sealed class Handler(IOptions<SiteOptions> siteOptions, IOptions<FloorOptions> floorOptions)
    : IQueryHandler<Query, GetSiteResponse>
{
    public Task<Result<GetSiteResponse>> Handle(Query query, CancellationToken cancellationToken)
    {
        var site = new SiteConfiguration(siteOptions.Value.Name, floorOptions.Value.Floors);
        return Task.FromResult(Result.Success(new GetSiteResponse(site)));
    }
}
