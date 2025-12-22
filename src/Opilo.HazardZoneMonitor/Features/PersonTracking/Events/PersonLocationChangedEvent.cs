using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public record PersonLocationChangedEvent(Guid PersonId, Location CurrentLocation) : IDomainEvent;

