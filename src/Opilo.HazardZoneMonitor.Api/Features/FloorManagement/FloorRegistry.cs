using Microsoft.Extensions.Options;

namespace Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

internal sealed class FloorRegistry : IFloorRegistry
{
    private readonly FloorConfiguration _configuration;

    public FloorRegistry(IOptions<FloorConfiguration> options)
    {
        _configuration = options.Value;
    }

    public IReadOnlyList<FloorDto> GetAllFloors() => _configuration.Floors;
}
