using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;

public sealed class PersonLocationChangedOnFloorEventArgs(FloorName floorName, PersonId personId, Coordinate location) : EventArgs
{
    public FloorName FloorName { get; } = floorName;
    public PersonId PersonId { get; } = personId;
    public Coordinate Location { get; } = location;
}
