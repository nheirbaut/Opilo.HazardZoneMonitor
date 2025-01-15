using Opilo.HazardZoneMonitor.Domain.Entities;

namespace Opilo.HazardZoneMonitor.Domain.Events;

public record PersonLocationChangedEvent(Person Person) : IDomainEvent;
