namespace Opilo.HazardZoneMonitor.Events.HazardZoneEvents;

public record PersonAddedToHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
