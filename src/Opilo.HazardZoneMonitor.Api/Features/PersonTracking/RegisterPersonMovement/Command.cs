using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed record Command(PersonId PersonId, double X, double Y) : ICommand<RegisteredPersonMovement>;
