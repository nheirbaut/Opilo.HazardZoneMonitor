using Microsoft.AspNetCore.Mvc;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<Command, Response>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/person-movements", async (
            [FromBody] Command command,
            ICommandHandler<Command, Response> handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.Handle(command, cancellationToken);
            return TypedResults.Created(
                new Uri($"/api/v1/person-movements/{response.Id}", UriKind.Relative),
                response);
        });
    }
}
