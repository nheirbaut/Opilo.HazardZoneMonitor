using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;

public sealed class PersonRemovedFromFloorEventArgs(FloorName floorName, PersonId personId) : EventArgs
{
    public FloorName FloorName { get; } = floorName;
    public PersonId PersonId { get; } = personId;
}

