using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public record PersonAddedToHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
