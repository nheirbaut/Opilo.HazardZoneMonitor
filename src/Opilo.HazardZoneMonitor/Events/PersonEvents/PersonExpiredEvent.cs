namespace Opilo.HazardZoneMonitor.Events.PersonEvents;

public record PersonExpiredEvent(Guid PersonId) : IDomainEvent;
