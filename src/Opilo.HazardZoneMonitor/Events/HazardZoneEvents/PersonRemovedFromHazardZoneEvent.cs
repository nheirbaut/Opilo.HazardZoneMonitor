﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Events.HazardZoneEvents;

public record PersonRemovedFromHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
