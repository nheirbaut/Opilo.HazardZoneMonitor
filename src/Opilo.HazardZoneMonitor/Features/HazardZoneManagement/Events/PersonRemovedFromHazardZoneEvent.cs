using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public sealed record PersonRemovedFromHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
