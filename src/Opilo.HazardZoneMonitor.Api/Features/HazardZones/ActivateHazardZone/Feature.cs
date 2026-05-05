using Microsoft.AspNetCore.Mvc;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.ActivateHazardZone;

public class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {

    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/hazard-zones/{hazardZoneName}/activate", async Task<IResult> (
            [FromRoute] string hazardZoneName,
            CancellationToken cancellationToken) =>
        {
            return TypedResults.NotFound();
        });
    }
}
