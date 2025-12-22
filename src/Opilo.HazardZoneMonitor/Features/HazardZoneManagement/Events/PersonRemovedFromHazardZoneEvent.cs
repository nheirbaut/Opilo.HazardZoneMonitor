using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public record PersonRemovedFromHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
