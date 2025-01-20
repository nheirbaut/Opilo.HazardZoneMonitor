namespace Opilo.HazardZoneMonitor.Domain.Events;

public record PersonRemovedFromFloorEvent(string FloorName, Guid PersonId) : IDomainEvent;
