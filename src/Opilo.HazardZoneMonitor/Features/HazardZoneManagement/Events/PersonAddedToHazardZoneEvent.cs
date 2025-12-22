using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public sealed record PersonAddedToHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
