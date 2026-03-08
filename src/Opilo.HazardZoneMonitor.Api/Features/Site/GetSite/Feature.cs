using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SiteOptions>(configuration.GetSection(nameof(SiteOptions)));

        services.AddScoped<IQueryHandler<Query, GetSiteResponse>, Handler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/site", async (
            IQueryHandler<Query, GetSiteResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new Query(), cancellationToken);
            return TypedResults.Ok(result.Value);
        });
    }
}
