using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed class GetFloorsFeature : IFeature
{
    public void AddServices(IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetFloorsQuery, GetFloorsResponse>, GetFloorsHandler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/floors", (IOptions<FloorOptions> floorConfiguration)
            => new GetFloorsResponse(floorConfiguration.Value.Floors));
    }
}
