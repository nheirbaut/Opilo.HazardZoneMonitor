﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Events.PersonEvents;

public record PersonCreatedEvent(Guid PersonId, Location Location) : IDomainEvent;
