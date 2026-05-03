using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public record PersonLocationUpdate(PersonId PersonId, Location Location);

