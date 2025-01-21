namespace Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;

public record PersonCreatedEvent(Entities.Person Person) : IDomainEvent;
