using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.ActivateHazardZone;

public sealed class Handler(IOptions<HazardZoneOptions> hazardZoneOptions) : ICommandHandler<Command>
{
    public Task<Result> Handle(Command command, CancellationToken cancellationToken)
    {
        var hazardZones = hazardZoneOptions.Value.HazardZones;
        if (hazardZones.Any(hazardZone => hazardZone.Name == command.HazardZoneName))
        {
            return Task.FromResult(Result.Success());
        }

        return Task.FromResult(Result.NotFound());
    }
}
