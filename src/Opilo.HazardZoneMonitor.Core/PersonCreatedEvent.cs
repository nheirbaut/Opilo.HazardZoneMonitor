namespace Opilo.HazardZoneMonitor.Core;

public record PersonCreatedEvent(Person Person) : IDomainEvent;
