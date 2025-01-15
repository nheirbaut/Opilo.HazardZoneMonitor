using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Events;

public record PersonAddedToFloorEvent(string FloorName, Guid PersonId, Location Location) : IDomainEvent;
