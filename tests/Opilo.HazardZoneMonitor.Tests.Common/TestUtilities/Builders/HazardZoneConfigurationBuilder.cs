using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

internal sealed class HazardZoneConfigurationBuilder
{
    public static readonly HazardZoneName DefaultName = HazardZoneName.From("Zone");

    public static readonly IReadOnlyList<Coordinate> DefaultOutline =
    [
        new(1, 1),
        new(2, 1),
        new(1, 2)
    ];

    private HazardZoneName _name = DefaultName;
    private IReadOnlyList<Coordinate> _outline = DefaultOutline;
    private TimeSpan _activationDuration = TimeSpan.Zero;
    private TimeSpan _preAlarmDuration = TimeSpan.Zero;
    private int _allowedNumberOfPersons;

    public static HazardZoneConfigurationBuilder Create() => new();

    public static HazardZoneConfiguration BuildSimple() =>
        new(DefaultName, DefaultOutline, TimeSpan.Zero, TimeSpan.Zero);

    public HazardZoneConfigurationBuilder WithName(string name)
    {
        _name = HazardZoneName.From(name);
        return this;
    }

    public HazardZoneConfigurationBuilder WithOutline(params Coordinate[] outline)
    {
        _outline = outline;
        return this;
    }

    public HazardZoneConfigurationBuilder WithRectangleOutline(double minX, double minY, double maxX, double maxY)
    {
        _outline = OutlineBuilder.Rectangle(minX, minY, maxX, maxY);
        return this;
    }

    public HazardZoneConfigurationBuilder WithTriangleOutline(
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

    public HazardZoneConfigurationBuilder WithActivationDuration(TimeSpan duration)
    {
        _activationDuration = duration;
        return this;
    }

    public HazardZoneConfigurationBuilder WithPreAlarmDuration(TimeSpan duration)
    {
        _preAlarmDuration = duration;
        return this;
    }

    public HazardZoneConfigurationBuilder WithAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        _allowedNumberOfPersons = allowedNumberOfPersons;
        return this;
    }

    public HazardZoneConfiguration Build() =>
        new(_name, _outline, _activationDuration, _preAlarmDuration, _allowedNumberOfPersons);
}
