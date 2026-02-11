using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Handler(IMovementsRepository movementsRepository, IClock clock) : ICommandHandler<Command, RegisteredPersonMovement>
{
    public async Task<Result<RegisteredPersonMovement>> Handle(Command command, CancellationToken cancellationToken)
    {
        DateTime registeredAt = clock.UtcNow;
        return await movementsRepository.RegisterMovementAsync(command.PersonId, command.X, command.Y, registeredAt, cancellationToken);
    }
}
