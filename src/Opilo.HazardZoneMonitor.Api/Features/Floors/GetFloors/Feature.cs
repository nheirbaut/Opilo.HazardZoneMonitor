using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IQueryHandler<Query, GetFloorsResponse>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/floors", async (
            IQueryHandler<Query, GetFloorsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new Query(), cancellationToken);
            return TypedResults.Ok(result.Value);
        });
    }
}
