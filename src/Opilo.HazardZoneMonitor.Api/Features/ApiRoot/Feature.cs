using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.ApiRoot;

public sealed class Feature : IFeature
{
    private const string ApiRoutePrefix = "/api/v1/";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // No services to register.
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/", (EndpointDataSource endpointDataSource) => Results.Json(new
        {
            Name = "HazardZone Monitor API",
            Version = "v1",
            Links = endpointDataSource.Endpoints
                .OfType<RouteEndpoint>()
                .Where(endpoint => endpoint.RoutePattern.RawText != null)
                .Where(endpoint => endpoint.RoutePattern.RawText!.StartsWith(ApiRoutePrefix, StringComparison.Ordinal))
                .Select(endpoint => new
                {
                    Rel = endpoint.RoutePattern.RawText![ApiRoutePrefix.Length..],
                    Href = endpoint.RoutePattern.RawText!,
                })
                .OrderBy(link => link.Rel, StringComparer.Ordinal)
                .ToArray(),
        }));
    }
}
