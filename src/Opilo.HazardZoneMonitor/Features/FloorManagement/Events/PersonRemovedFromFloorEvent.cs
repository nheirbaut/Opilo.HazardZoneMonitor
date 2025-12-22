using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.FloorManagement.Events;

public sealed record PersonRemovedFromFloorEvent(string FloorName, Guid PersonId) : IDomainEvent;

