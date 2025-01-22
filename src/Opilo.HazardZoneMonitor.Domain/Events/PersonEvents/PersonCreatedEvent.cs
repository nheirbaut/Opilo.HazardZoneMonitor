using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;

public record PersonCreatedEvent(Guid PersonId, Location Location) : IDomainEvent;
