namespace Opilo.HazardZoneMonitor.Core;

public record PersonExpiredEvent(Person Person) : IDomainEvent;
