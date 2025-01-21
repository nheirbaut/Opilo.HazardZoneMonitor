namespace Opilo.HazardZoneMonitor.Domain.Events.FloorEvents;

public record PersonRemovedFromFloorEvent(string FloorName, Guid PersonId) : IDomainEvent;
