using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public sealed record PersonLocationChangedEvent(Guid PersonId, Location CurrentLocation) : IDomainEvent;

