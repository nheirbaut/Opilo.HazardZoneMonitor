using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;

public interface IHazardZoneService
{
    IReadOnlyList<HazardZoneConfiguration> GetHazardZones();

    Result ActivateHazardZone(HazardZoneName hazardZoneName);
}
