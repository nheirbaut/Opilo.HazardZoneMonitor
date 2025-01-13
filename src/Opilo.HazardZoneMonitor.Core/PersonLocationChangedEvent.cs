namespace Opilo.HazardZoneMonitor.Core;

public record PersonLocationChangedEvent(Person Person) : IDomainEvent;
