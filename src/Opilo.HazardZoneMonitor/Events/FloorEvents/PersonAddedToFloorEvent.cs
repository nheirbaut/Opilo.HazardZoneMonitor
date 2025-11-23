using Opilo.HazardZoneMonitor.ValueObjects;

namespace Opilo.HazardZoneMonitor.Events.FloorEvents;

public record PersonAddedToFloorEvent(string FloorName, Guid PersonId, Location Location) : IDomainEvent;
