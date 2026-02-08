namespace Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;

public sealed class PersonRemovedFromFloorEventArgs(string floorName, Guid personId) : EventArgs
{
    public string FloorName { get; } = floorName;
    public Guid PersonId { get; } = personId;
}

