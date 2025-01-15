using Opilo.HazardZoneMonitor.Domain.Entities;

namespace Opilo.HazardZoneMonitor.Domain.Events;

public record PersonCreatedEvent(Person Person) : IDomainEvent;
