using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;

public sealed record Query(Guid Id) : IQuery<RegisteredPersonMovement>;
