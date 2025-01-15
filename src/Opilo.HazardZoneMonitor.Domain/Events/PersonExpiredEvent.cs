using Opilo.HazardZoneMonitor.Domain.Entities;

namespace Opilo.HazardZoneMonitor.Domain.Events;

public record PersonExpiredEvent(Person Person) : IDomainEvent;
