using Ardalis.Result;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;

public interface IHazardZoneService
{
    Result ActivateHazardZone(HazardZoneName hazardZoneName);
}
