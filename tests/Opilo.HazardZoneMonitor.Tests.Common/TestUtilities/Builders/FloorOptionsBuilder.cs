using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

internal sealed class FloorOptionsBuilder
{
    private IReadOnlyList<FloorConfiguration> _floors = [];

    public static FloorOptionsBuilder Create() => new();

    public FloorOptionsBuilder WithFloor(string name, Action<FloorConfigurationBuilder>? configure = null)
    {
        var builder = FloorConfigurationBuilder.Create().WithName(name);
        configure?.Invoke(builder);

        _floors = [.. _floors, builder.Build()];
        return this;
    }

    public FloorOptionsBuilder WithFloor(FloorConfiguration floor)
    {
        _floors = [.. _floors, floor];
        return this;
    }

    public FloorOptionsBuilder WithFloors(params FloorConfiguration[] floors)
    {
        _floors = [.. _floors, .. floors];
        return this;
    }

    public FloorOptions Build() => new()
    {
        Floors = _floors,
    };
}
