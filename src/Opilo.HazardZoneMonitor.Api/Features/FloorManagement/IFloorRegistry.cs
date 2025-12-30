namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

public interface IFloorRegistry
{
    IReadOnlyList<FloorDto> GetAllFloors();
}
