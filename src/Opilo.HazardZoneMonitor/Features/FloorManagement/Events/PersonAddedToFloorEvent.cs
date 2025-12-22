using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.FloorManagement.Events;

public record PersonAddedToFloorEvent(string FloorName, Guid PersonId, Location Location) : IDomainEvent;

