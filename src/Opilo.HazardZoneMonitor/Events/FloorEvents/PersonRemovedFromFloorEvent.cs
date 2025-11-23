namespace Opilo.HazardZoneMonitor.Events.FloorEvents;

public record PersonRemovedFromFloorEvent(string FloorName, Guid PersonId) : IDomainEvent;
