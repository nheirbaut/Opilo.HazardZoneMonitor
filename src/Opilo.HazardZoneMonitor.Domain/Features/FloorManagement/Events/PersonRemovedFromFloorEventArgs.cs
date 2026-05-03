using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;

public sealed class PersonRemovedFromFloorEventArgs(string floorName, PersonId personId) : EventArgs
{
    public string FloorName { get; } = floorName;
    public PersonId PersonId { get; } = personId;
}

