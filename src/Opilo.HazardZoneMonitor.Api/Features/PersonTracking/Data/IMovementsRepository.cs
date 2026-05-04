using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.Data;

public interface IMovementsRepository
{
    Task<Result<RegisteredPersonMovement>> RegisterMovementAsync(PersonId personId, Coordinate coordinate, DateTime registeredAt, CancellationToken cancellationToken);
    Task<Result<RegisteredPersonMovement>> GetMovementByIdAsync(Guid id, CancellationToken cancellationToken);
}
