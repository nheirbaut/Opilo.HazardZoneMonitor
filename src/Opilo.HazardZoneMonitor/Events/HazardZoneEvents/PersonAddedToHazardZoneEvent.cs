﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Events.HazardZoneEvents;

public record PersonAddedToHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
