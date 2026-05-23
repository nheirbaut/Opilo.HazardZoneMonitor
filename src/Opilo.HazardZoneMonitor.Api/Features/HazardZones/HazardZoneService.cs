using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed class HazardZoneService : IHazardZoneService
{
    public Result ActivateHazardZone(HazardZoneName hazardZoneName)
    {
        return Result.NotFound();
    }
}
