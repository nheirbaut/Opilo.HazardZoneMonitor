namespace Opilo.HazardZoneMonitor.Events.HazardZoneEvents;

public record HazardZoneActivationStartedEvent(string HazardZoneName) : IDomainEvent;
