using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;

public record PersonLocationChangedEvent(Guid PersonId, Location CurrentLocation) : IDomainEvent;
