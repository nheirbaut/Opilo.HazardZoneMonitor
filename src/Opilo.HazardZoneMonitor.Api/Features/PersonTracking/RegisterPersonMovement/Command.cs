using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed record Command(PersonId PersonId, Coordinate Coordinate) : ICommand<RegisteredPersonMovement>;
