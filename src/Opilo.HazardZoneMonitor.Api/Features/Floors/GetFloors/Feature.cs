using Ardalis.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FloorOptions>(configuration.GetSection(nameof(FloorOptions)));
        services.AddSingleton<IValidateOptions<FloorOptions>, FloorOptionsValidator>();
        services.AddOptions<FloorOptions>().ValidateOnStart();

        services.AddScoped<IQueryHandler<Query, GetFloorsResponse>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/floors", async Task<Results<Ok<GetFloorsResponse>, StatusCodeHttpResult>> (
            IQueryHandler<Query, GetFloorsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new Query(), cancellationToken);

            if (result.Status != ResultStatus.Ok)
            {
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            }

            return TypedResults.Ok(result.Value);
        });
    }
}
