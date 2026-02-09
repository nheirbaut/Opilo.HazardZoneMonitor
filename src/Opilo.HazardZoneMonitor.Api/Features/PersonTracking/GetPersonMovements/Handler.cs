using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetPersonMovements;

public sealed class Handler : IQueryHandler<Query, Response>
{
    public Task<Result<Response>> Handle(Query query, CancellationToken cancellationToken)
    {
        if (!RegisterPersonMovement.Handler.Movements.TryGetValue(query.Id, out var movement))
            return Task.FromResult(Result<Response>.NotFound());

        return Task.FromResult(Result.Success(new Response(movement.PersonId, movement.X, movement.Y, movement.RegisteredAt)));
    }
}
