﻿﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Events.FloorEvents;

#pragma warning disable S1133 // Deprecated code should be removed eventually
[Obsolete("Use Opilo.HazardZoneMonitor.Features.FloorManagement.Events.PersonRemovedFromFloorEvent instead")]
public record PersonRemovedFromFloorEvent(string FloorName, Guid PersonId) : IDomainEvent;
#pragma warning restore S1133
