using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<Query, Response>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/floors", (IOptions<FloorOptions> floorConfiguration)
            => new Response(floorConfiguration.Value.Floors));
    }
}
