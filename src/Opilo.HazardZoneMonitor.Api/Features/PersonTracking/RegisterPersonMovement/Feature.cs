using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/person-movements", (Command command)
            => TypedResults.Created(
                new Uri("/api/v1/person-movements/1", UriKind.Relative),
                new Response(command.PersonId)));
    }
}
