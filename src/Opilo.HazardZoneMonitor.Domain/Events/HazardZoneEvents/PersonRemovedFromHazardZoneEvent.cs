namespace Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;

public record PersonRemovedFromHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
