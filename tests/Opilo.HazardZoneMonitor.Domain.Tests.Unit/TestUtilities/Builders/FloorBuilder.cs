using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities.Builders;

internal sealed class FloorBuilder
{
    private FloorName _name = DefaultName;
    private Outline _outline = DefaultOutline;
    private readonly List<HazardZone> _hazardZones = [];
    private TimeSpan? _personLifespan;
    private ITimerFactory? _timerFactory;

    public static readonly FloorName DefaultName = FloorName.From("TestFloor");

    public static readonly Outline DefaultOutline = new(new([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]));

    public static Floor BuildSimple() => new(DefaultName, DefaultOutline, []);

    public static FloorBuilder Create() => new();

    public FloorBuilder WithName(FloorName name)
    {
        _name = name;
        return this;
    }

    public FloorBuilder WithOutline(Outline outline)
    {
        _outline = outline;
        return this;
    }

    public FloorBuilder WithHazardZone(HazardZone hazardZone)
    {
        _hazardZones.Add(hazardZone);
        return this;
    }

    public FloorBuilder WithHazardZones(IEnumerable<HazardZone> hazardZones)
    {
        _hazardZones.AddRange(hazardZones);
        return this;
    }

    public FloorBuilder WithPersonLifespan(TimeSpan personLifespan)
    {
        _personLifespan = personLifespan;
        return this;
    }

    public FloorBuilder WithTimerFactory(ITimerFactory timerFactory)
    {
        _timerFactory = timerFactory;
        return this;
    }

    public Floor Build()
    {
        return new Floor(_name, _outline, _hazardZones, _personLifespan, _timerFactory);
    }

    private FloorBuilder()
    {
    }
}
