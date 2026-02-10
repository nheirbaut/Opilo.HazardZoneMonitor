using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Handler(IMovementsRepository movementsRepository) : ICommandHandler<Command, RegisteredPersonMovement>
{
    public async Task<Result<RegisteredPersonMovement>> Handle(Command command, CancellationToken cancellationToken)
    {
        DateTime registeredAt = DateTime.UtcNow;
        return await movementsRepository.RegisterMovementAsync(command.PersonId, command.X, command.Y, registeredAt, cancellationToken);
    }
}
