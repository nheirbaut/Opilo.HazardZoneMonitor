using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

internal sealed class HazardZoneConfigurationBuilder
{
    public const string DefaultName = "Zone";

    public static readonly IReadOnlyList<PointConfiguration> DefaultOutline =
    [
        new PointConfiguration(1, 1),
        new PointConfiguration(2, 1),
        new PointConfiguration(1, 2)
    ];

    private string _name = DefaultName;
    private IReadOnlyList<PointConfiguration> _outline = DefaultOutline;
    private TimeSpan _activationDuration = TimeSpan.Zero;
    private TimeSpan _preAlarmDuration = TimeSpan.Zero;
    private ZoneState _zoneState;
    private AlarmState _alarmState;
    private int _allowedNumberOfPersons;

    public static HazardZoneConfigurationBuilder Create() => new();

    public static HazardZoneConfiguration BuildSimple() =>
        new(DefaultName, DefaultOutline, TimeSpan.Zero, TimeSpan.Zero);

    public HazardZoneConfigurationBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public HazardZoneConfigurationBuilder WithOutline(params PointConfiguration[] outline)
    {
        _outline = outline;
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

    public HazardZoneConfigurationBuilder WithZoneState(ZoneState zoneState)
    {
        _zoneState = zoneState;
        return this;
    }

    public HazardZoneConfigurationBuilder WithAlarmState(AlarmState alarmState)
    {
        _alarmState = alarmState;
        return this;
    }

    public HazardZoneConfigurationBuilder WithAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        _allowedNumberOfPersons = allowedNumberOfPersons;
        return this;
    }

    public HazardZoneConfiguration Build() =>
        new(_name, _outline, _activationDuration, _preAlarmDuration, _zoneState, _alarmState, _allowedNumberOfPersons);
}
