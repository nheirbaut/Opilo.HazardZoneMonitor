namespace Opilo.HazardZoneMonitor.Api.Shared.Features;

public interface IFeature
{
    void AddServices(IServiceCollection services, IConfiguration configuration);
    void MapEndpoints(IEndpointRouteBuilder app);
}
