using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SiteOptions>(configuration.GetSection(nameof(SiteOptions)));
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/site", (IOptions<SiteOptions> siteOptions, IOptions<FloorOptions> floorOptions) =>
        {
            var site = new SiteConfiguration(siteOptions.Value.Name ?? string.Empty, floorOptions.Value.Floors);
            return TypedResults.Ok(new GetSiteResponse(site));
        });
    }
}
