using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetPersonMovements;

public sealed class Handler : IQueryHandler<Query, Response>
{
    public Task<Response> Handle(Query query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Response(query.PersonId));
    }
}
