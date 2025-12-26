using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.FloorManagement.Events;

public sealed class PersonAddedToFloorEventArgs(string floorName, Guid personId, Location location) : EventArgs
{
    public string FloorName { get; } = floorName;
    public Guid PersonId { get; } = personId;
    public Location Location { get; } = location;
}

