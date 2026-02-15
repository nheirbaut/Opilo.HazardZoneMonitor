using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;

public sealed class Handler(IMovementsRepository movementsRepository) : IQueryHandler<Query, RegisteredPersonMovement>
{
    public async Task<Result<RegisteredPersonMovement>> Handle(Query query, CancellationToken cancellationToken)
    {
        return await movementsRepository.GetMovementByIdAsync(query.Id, cancellationToken);
    }
}
