using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;

public sealed class Feature: IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Nothing to add here yet
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/hazard-zones", () =>
        {
            return TypedResults.Ok(new Response([]));
        });
    }
}
