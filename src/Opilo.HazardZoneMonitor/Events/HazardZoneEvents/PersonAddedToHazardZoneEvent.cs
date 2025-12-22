using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Events.HazardZoneEvents;

#pragma warning disable S1133 // Deprecated code should be removed eventually
[Obsolete("Use Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events.PersonAddedToHazardZoneEvent instead")]

public record PersonAddedToHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
#pragma warning restore S1133
