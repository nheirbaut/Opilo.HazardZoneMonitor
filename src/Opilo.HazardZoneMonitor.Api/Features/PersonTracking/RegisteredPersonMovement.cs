using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

public sealed class RegisteredPersonMovement : IResponse
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required Guid PersonId { get; init; }
    public required Coordinate Coordinate { get; init; }
    public required DateTime RegisteredAt { get; init; }
}
