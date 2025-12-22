﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Events.HazardZoneEvents;

public record HazardZoneActivationStartedEvent(string HazardZoneName) : IDomainEvent;
