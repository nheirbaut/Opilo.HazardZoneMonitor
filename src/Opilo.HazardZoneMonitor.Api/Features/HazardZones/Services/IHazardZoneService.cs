using Ardalis.Result;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;

public interface IHazardZoneService
{
    IReadOnlyList<HazardZoneInfo> GetHazardZones();

    void ApplyPersonLocationUpdate(PersonLocationUpdate personLocationUpdate);

    void RemovePerson(PersonId personId);

    Result ActivateHazardZone(HazardZoneName hazardZoneName);
    Result DeactivateHazardZone(HazardZoneName hazardZoneName);
}
