using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HazardZoneOptions>(configuration.GetSection(nameof(HazardZoneOptions)));

        services.AddScoped<IQueryHandler<Query, GetHazardZonesResponse>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/hazard-zones", async (
            IQueryHandler<Query, GetHazardZonesResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new Query(), cancellationToken);
            return TypedResults.Ok(result.Value);
        });
    }
}
