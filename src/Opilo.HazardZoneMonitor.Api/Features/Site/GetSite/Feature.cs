using Microsoft.Extensions.Options;
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
        app.MapGet("/api/v1/site", (IOptions<SiteOptions> options) =>
        {
            var siteOptions = options.Value;
            var site = new SiteConfiguration(siteOptions.Name ?? string.Empty, []);
            return Results.Ok(new Response(site));
        });
    }
}
