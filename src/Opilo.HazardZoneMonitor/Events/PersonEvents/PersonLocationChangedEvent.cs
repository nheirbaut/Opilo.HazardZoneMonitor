﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Events.PersonEvents;

public record PersonLocationChangedEvent(Guid PersonId, Location CurrentLocation) : IDomainEvent;
