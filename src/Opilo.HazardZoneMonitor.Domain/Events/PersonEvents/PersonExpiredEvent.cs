namespace Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;

public record PersonExpiredEvent(Guid PersonId) : IDomainEvent;
