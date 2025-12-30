using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

public sealed record RegisterPersonMovementRequest(Guid PersonId, Location Location);
