using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public record PersonCreatedEvent(Guid PersonId, Location Location) : IDomainEvent;

