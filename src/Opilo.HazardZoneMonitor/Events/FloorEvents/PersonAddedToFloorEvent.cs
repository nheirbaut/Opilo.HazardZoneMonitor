﻿﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Events.FloorEvents;

#pragma warning disable S1133 // Deprecated code should be removed eventually
[Obsolete("Use Opilo.HazardZoneMonitor.Features.FloorManagement.Events.PersonAddedToFloorEvent instead")]
public record PersonAddedToFloorEvent(string FloorName, Guid PersonId, Location Location) : IDomainEvent;
#pragma warning restore S1133
