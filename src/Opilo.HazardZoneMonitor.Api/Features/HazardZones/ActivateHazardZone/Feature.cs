using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.ActivateHazardZone;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICommandHandler<Command>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/hazard-zones/{hazardZoneName}/activate", async Task<IResult> (
            [FromRoute] string hazardZoneName,
            ICommandHandler<Command> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new Command(HazardZoneName.From(hazardZoneName)), cancellationToken);

            return result.Status switch
            {
                ResultStatus.NotFound => TypedResults.NotFound(),
                ResultStatus.Ok => TypedResults.NoContent(),
                _ => TypedResults.InternalServerError()
            };
        });
    }
}
