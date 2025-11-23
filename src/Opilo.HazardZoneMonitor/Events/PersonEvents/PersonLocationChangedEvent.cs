using Opilo.HazardZoneMonitor.ValueObjects;

namespace Opilo.HazardZoneMonitor.Events.PersonEvents;

public record PersonLocationChangedEvent(Guid PersonId, Location CurrentLocation) : IDomainEvent;
