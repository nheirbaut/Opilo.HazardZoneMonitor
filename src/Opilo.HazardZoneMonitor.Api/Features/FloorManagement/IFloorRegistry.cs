namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

internal interface IFloorRegistry
{
    IReadOnlyList<FloorDto> GetAllFloors();
}
