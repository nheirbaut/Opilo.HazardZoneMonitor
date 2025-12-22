﻿﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Events.PersonEvents;

#pragma warning disable S1133 // Deprecated code should be removed eventually
[Obsolete("Use Opilo.HazardZoneMonitor.Features.PersonTracking.Events.PersonExpiredEvent instead")]
public record PersonExpiredEvent(Guid PersonId) : IDomainEvent;
#pragma warning restore S1133
