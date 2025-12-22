﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Events.FloorEvents;

public record PersonAddedToFloorEvent(string FloorName, Guid PersonId, Location Location) : IDomainEvent;
