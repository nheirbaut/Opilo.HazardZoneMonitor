using Ardalis.Result;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

public interface IMovementsRepository
{
    Task<Result<RegisteredPersonMovement>> RegisterMovementAsync(PersonId personId, double x, double y, DateTime registeredAt, CancellationToken cancellationToken);
    Task<Result<RegisteredPersonMovement>> GetMovementByIdAsync(Guid id, CancellationToken cancellationToken);
}
