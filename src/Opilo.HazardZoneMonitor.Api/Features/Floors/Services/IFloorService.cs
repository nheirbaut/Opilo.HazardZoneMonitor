using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.Services;

public interface IFloorService
{
    void ApplyPersonLocationUpdate(PersonLocationUpdate personLocationUpdate);
}
