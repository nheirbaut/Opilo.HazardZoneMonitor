using System.Collections.Concurrent;
using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared;

public sealed class InMemoryMovementsRepository : IMovementsRepository
{
    private readonly ConcurrentDictionary<Guid, RegisteredPersonMovement> _movements = new();

    public Task<Result<RegisteredPersonMovement>> RegisterMovementAsync(Guid personId, double x, double y, DateTime registeredAt, CancellationToken cancellationToken)
    {
        RegisteredPersonMovement movement = new()
        {
            PersonId = personId,
            X = x,
            Y = y,
            RegisteredAt = registeredAt,
        };

        _movements[movement.Id] = movement;
        return Task.FromResult(Result.Created(movement));
    }

    public Task<Result<RegisteredPersonMovement>> GetMovementByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (_movements.TryGetValue(id, out RegisteredPersonMovement? movement))
        {
            return Task.FromResult(Result.Success(movement));
        }

        return Task.FromResult(Result<RegisteredPersonMovement>.NotFound());
    }
}
