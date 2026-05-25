using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHazardZoneService, HazardZoneService>();
        services.AddHostedService<HazardZoneInitializationHostedService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Infrastructure feature — no endpoints to map.
    }
}
