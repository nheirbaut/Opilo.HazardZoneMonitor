using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // No services to register.
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/site", () => Results.Ok());
    }
}
