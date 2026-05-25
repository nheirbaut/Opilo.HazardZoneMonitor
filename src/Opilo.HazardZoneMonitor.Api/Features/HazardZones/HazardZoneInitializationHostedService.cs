using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed class HazardZoneInitializationHostedService(IHazardZoneService hazardZoneService) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Ensure HazardZones are activated at startup
        _ = hazardZoneService.GetHazardZones();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
