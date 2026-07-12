using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Services;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.Data;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

public sealed class Handler(IMovementsRepository movementsRepository, IClock clock, IFloorService floorService) : ICommandHandler<Command, RegisteredPersonMovement>
{
    public async Task<Result<RegisteredPersonMovement>> Handle(Command command, CancellationToken cancellationToken)
    {
        var registeredAt = clock.UtcNow;
        var result = await movementsRepository.RegisterMovementAsync(command.PersonId, command.Coordinate, registeredAt, cancellationToken);

        if (result.Status == ResultStatus.Created)
        {
            floorService.ApplyPersonLocationUpdate(new PersonLocationUpdate(command.PersonId, command.Coordinate));
        }

        return result;
    }
}
