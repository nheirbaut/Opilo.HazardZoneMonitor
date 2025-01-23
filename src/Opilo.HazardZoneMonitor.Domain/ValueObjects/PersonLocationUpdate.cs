using Opilo.HazardZoneMonitor.Domain.Events;

namespace Opilo.HazardZoneMonitor.Domain.ValueObjects;

public record PersonLocationUpdate(Guid PersonId, Location Location) : IDomainEvent;
