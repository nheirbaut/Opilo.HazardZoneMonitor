namespace Opilo.HazardZoneMonitor.Domain.Events;

public record PersonExpiredEvent(Guid PersonId) : IDomainEvent;
