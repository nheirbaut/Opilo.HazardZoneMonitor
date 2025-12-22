﻿﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Events.PersonEvents;

#pragma warning disable S1133 // Deprecated code should be removed eventually
[Obsolete("Use Opilo.HazardZoneMonitor.Features.PersonTracking.Events.PersonCreatedEvent instead")]
public record PersonCreatedEvent(Guid PersonId, Location Location) : IDomainEvent;
#pragma warning restore S1133
