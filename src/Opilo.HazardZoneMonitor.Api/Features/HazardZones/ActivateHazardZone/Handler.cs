using Ardalis.Result;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.ActivateHazardZone;

public sealed class Handler(IHazardZoneService hazardZoneService) : ICommandHandler<Command>
{
    public Task<Result> Handle(Command command, CancellationToken cancellationToken)
    {
        return Task.FromResult(hazardZoneService.ActivateHazardZone(command.HazardZoneName));
    }
}
