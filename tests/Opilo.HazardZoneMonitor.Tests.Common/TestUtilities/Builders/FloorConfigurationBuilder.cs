using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

internal sealed class FloorConfigurationBuilder
{
    public const string DefaultName = "Floor";

    public static readonly IReadOnlyList<PointConfiguration> DefaultOutline =
    [
        new PointConfiguration(0, 0),
        new PointConfiguration(10, 0),
        new PointConfiguration(0, 10)
    ];

    private string _name = DefaultName;
    private IReadOnlyList<PointConfiguration> _outline = DefaultOutline;
    private IReadOnlyList<HazardZoneConfiguration> _hazardZones = [];

    public static FloorConfigurationBuilder Create() => new();

    public static FloorConfiguration BuildSimple() =>
        new(DefaultName, DefaultOutline);

    public FloorConfigurationBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public FloorConfigurationBuilder WithOutline(params PointConfiguration[] outline)
    {
        _outline = outline;
        return this;
    }

    public FloorConfigurationBuilder WithHazardZones(params HazardZoneConfiguration[] hazardZones)
    {
        _hazardZones = hazardZones;
        return this;
    }

    public FloorConfiguration Build() =>
        new(_name, _outline)
        {
            HazardZones = _hazardZones
        };
}
