﻿#pragma warning disable CA1716 // Identifiers should not match keywords - "Shared" is intentional for architecture

namespace Opilo.HazardZoneMonitor.Shared.Primitives;

public record PersonLocationUpdate(Guid PersonId, Location Location);

