using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;

public sealed class PersonAddedToFloorEventArgs(FloorName floorName, PersonId personId, Location location) : EventArgs
{
    public FloorName FloorName { get; } = floorName;
    public PersonId PersonId { get; } = personId;
    public Location Location { get; } = location;
}

