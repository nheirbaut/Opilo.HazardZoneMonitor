namespace Opilo.HazardZoneMonitor.Api.Shared.Features;

public interface IFeature
{
    void AddServices(IServiceCollection services);
    void MapEndpoints(IEndpointRouteBuilder app);
}
