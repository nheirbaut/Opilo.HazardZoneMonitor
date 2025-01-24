namespace Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;

public record HazardZoneActivationStartedEvent(string HazardZoneName) : IDomainEvent;
