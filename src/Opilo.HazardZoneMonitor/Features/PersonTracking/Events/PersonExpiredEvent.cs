using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public record PersonExpiredEvent(Guid PersonId) : IDomainEvent;

