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
                .Select(endpoint => endpoint.RoutePattern.RawText)
                .Where(text => text is not null)
                .Select(text => text!)
                .Where(text => text.StartsWith(ApiRoutePrefix, StringComparison.Ordinal))
                .Where(text => !text.Contains('{', StringComparison.Ordinal))
                .Select(text => new
                {
                    Rel = text[ApiRoutePrefix.Length..],
                    Href = text,
                })
                .OrderBy(link => link.Rel, StringComparer.Ordinal)
                .ToArray(),
        }));
    }
}
