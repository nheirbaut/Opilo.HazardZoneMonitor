using Ardalis.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SiteOptions>(configuration.GetSection(nameof(SiteOptions)));
        services.AddSingleton<IValidateOptions<SiteOptions>, SiteOptionsValidator>();
        services.AddOptions<SiteOptions>().ValidateOnStart();

        services.AddScoped<IQueryHandler<Query, GetSiteResponse>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/site", async Task<Results<Ok<GetSiteResponse>, StatusCodeHttpResult>> (
            IQueryHandler<Query, GetSiteResponse> handler,
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
