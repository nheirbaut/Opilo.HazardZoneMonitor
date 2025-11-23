using Opilo.HazardZoneMonitor.ValueObjects;

namespace Opilo.HazardZoneMonitor.Events.PersonEvents;

public record PersonCreatedEvent(Guid PersonId, Location Location) : IDomainEvent;
