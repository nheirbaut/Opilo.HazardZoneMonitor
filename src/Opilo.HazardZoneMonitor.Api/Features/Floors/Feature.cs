using Opilo.HazardZoneMonitor.Api.Features.Floors.Services;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFloorService, FloorService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Infrastructure feature — no endpoints to map.
    }
}
