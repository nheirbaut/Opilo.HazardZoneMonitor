using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetPersonMovements;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<Query, Response>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/person-movements/{id:guid}", async (
            Guid id,
            IQueryHandler<Query, Response> handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.Handle(new Query(id), cancellationToken);
            return Results.Ok(response);
        });
    }
}
