using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

internal sealed class FloorConfigurationBuilder
{
    public static readonly FloorName DefaultName = FloorName.From("Floor");

    public static readonly IReadOnlyList<Coordinate> DefaultOutline =
    [
        new(0, 0),
        new(10, 0),
        new(0, 10)
    ];

    private FloorName _name = DefaultName;
    private IReadOnlyList<Coordinate> _outline = DefaultOutline;
    private IReadOnlyList<HazardZoneConfiguration> _hazardZones = [];

    public static FloorConfigurationBuilder Create() => new();

    public static FloorConfiguration BuildSimple() =>
        new(DefaultName, DefaultOutline);

    public FloorConfigurationBuilder WithName(string name)
    {
        _name = FloorName.From(name);
        return this;
    }

    public FloorConfigurationBuilder WithOutline(params Coordinate[] outline)
    {
        _outline = outline;
        return this;
    }

    public FloorConfigurationBuilder WithRectangleOutline(double minX, double minY, double maxX, double maxY)
    {
        _outline = OutlineBuilder.Rectangle(minX, minY, maxX, maxY);
        return this;
    }

    public FloorConfigurationBuilder WithTriangleOutline(
        double firstX,
        double firstY,
        double secondX,
        double secondY,
        double thirdX,
        double thirdY)
    {
        _outline = OutlineBuilder.Triangle(firstX, firstY, secondX, secondY, thirdX, thirdY);
        return this;
    }

    public FloorConfigurationBuilder WithHazardZones(params HazardZoneConfiguration[] hazardZones)
    {
        _hazardZones = hazardZones;
        return this;
    }

    public FloorConfigurationBuilder WithHazardZone(string name, Action<HazardZoneConfigurationBuilder>? configure = null)
    {
        var builder = HazardZoneConfigurationBuilder.Create().WithName(name);
        configure?.Invoke(builder);

        _hazardZones = [.. _hazardZones, builder.Build()];
        return this;
    }

    public FloorConfiguration Build() =>
        new(_name, _outline)
        {
            HazardZones = _hazardZones
        };
}
