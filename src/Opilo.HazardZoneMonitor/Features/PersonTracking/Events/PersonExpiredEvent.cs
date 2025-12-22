using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public sealed record PersonExpiredEvent(Guid PersonId) : IDomainEvent;

