﻿using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;

public record PersonAddedToHazardZoneEvent(Guid PersonId, string HazardZoneName) : IDomainEvent;
