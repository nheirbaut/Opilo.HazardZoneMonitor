using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetPersonMovements;

public sealed class Handler : IQueryHandler<Query, Response>
{
    public Task<Response> Handle(Query query, CancellationToken cancellationToken)
    {
        var movement = RegisterPersonMovement.Handler.Movements[query.Id];
        return Task.FromResult(new Response(movement.PersonId, movement.X, movement.Y, movement.RegisteredAt));
    }
}
