using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

public sealed class RegisteredPersonMovement : IResponse
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required PersonId PersonId { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }
    public required DateTime RegisteredAt { get; init; }
}
