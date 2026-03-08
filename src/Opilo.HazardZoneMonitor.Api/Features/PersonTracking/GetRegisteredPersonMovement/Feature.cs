using Ardalis.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IQueryHandler<Query, RegisteredPersonMovement>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/person-movements/{id:guid}", async Task<Results<Ok<RegisteredPersonMovement>, NotFound>> (
            Guid id,
            IQueryHandler<Query, RegisteredPersonMovement> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new Query(id), cancellationToken);

            if (result.Status == ResultStatus.NotFound)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(result.Value);
        });
    }
}
