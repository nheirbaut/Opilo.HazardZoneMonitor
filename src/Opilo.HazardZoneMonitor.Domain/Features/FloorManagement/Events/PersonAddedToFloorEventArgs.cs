using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;

public sealed class PersonAddedToFloorEventArgs(string floorName, PersonId personId, Location location) : EventArgs
{
    public string FloorName { get; } = floorName;
    public PersonId PersonId { get; } = personId;
    public Location Location { get; } = location;
}

