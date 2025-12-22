using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.FloorManagement.Events;

public record PersonRemovedFromFloorEvent(string FloorName, Guid PersonId) : IDomainEvent;

