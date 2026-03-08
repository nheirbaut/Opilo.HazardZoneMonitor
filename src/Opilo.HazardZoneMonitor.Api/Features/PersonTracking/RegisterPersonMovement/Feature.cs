using Ardalis.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICommandHandler<Command, RegisteredPersonMovement>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/person-movements", async Task<Results<Created<RegisteredPersonMovement>, StatusCodeHttpResult>> (
            [FromBody] Command command,
            ICommandHandler<Command, RegisteredPersonMovement> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(command, cancellationToken);

            if (result.Status != ResultStatus.Created)
            {
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            }

            return TypedResults.Created(
                new Uri($"/api/v1/person-movements/{result.Value.Id}", UriKind.Relative),
                result.Value);
        });
    }
}
