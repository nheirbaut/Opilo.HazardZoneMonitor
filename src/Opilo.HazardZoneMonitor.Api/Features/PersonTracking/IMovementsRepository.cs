using Ardalis.Result;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

public interface IMovementsRepository
{
    Task<Result<RegisteredPersonMovement>> RegisterMovementAsync(Guid personId, double x, double y, DateTime registeredAt, CancellationToken cancellationToken);
    Task<Result<RegisteredPersonMovement>> GetMovementByIdAsync(Guid id, CancellationToken cancellationToken);
}
