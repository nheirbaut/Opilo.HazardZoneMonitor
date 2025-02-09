using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities.Builders;

internal enum HazardZoneTestState
{
    Inactive,
    Active,
    PreAlarm
}

internal sealed class HazardZoneBuilder
{
    private HazardZoneTestState _desiredState = HazardZoneTestState.Inactive;
    private readonly List<string> _externalActivationSourceIds = new();
    private int _allowedNumberOfPersons;
    private TimeSpan _preAlarmDuration = DefaultPreAlarmDuration;

    public const string DefaultName = "HazardZone";

    public static readonly Outline DefaultOutline = new(new([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]));

    public static readonly TimeSpan DefaultPreAlarmDuration = TimeSpan.FromSeconds(5);

    public static HazardZone BuildSimple() => new(DefaultName, DefaultOutline, DefaultPreAlarmDuration);

    public static HazardZoneBuilder Create() => new();

    public HazardZoneBuilder WithExternalActivationSource(string sourceId)
    {
        _externalActivationSourceIds.Add(sourceId);
        return this;
    }

    public HazardZoneBuilder WithAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        _allowedNumberOfPersons = allowedNumberOfPersons;
        return this;
    }

    public HazardZoneBuilder WithState(HazardZoneTestState state)
    {
        _desiredState = state;
        return this;
    }

    public HazardZoneBuilder WithPreAlarmDuration(TimeSpan duration)
    {
        _preAlarmDuration = duration;
        return this;
    }

    public HazardZone Build()
    {
        var hazardZone = new HazardZone(DefaultName, DefaultOutline, _preAlarmDuration);
        hazardZone.SetAllowedNumberOfPersons(_allowedNumberOfPersons);

        foreach (var sourceId in _externalActivationSourceIds)
            hazardZone.ActivateFromExternalSource(sourceId);

        switch (_desiredState)
        {
            case HazardZoneTestState.Inactive:
                if (hazardZone.IsActive)
                    hazardZone.ManuallyDeactivate();
                break;
            case HazardZoneTestState.Active:
                if (hazardZone.IsActive)
                    hazardZone.ManuallyDeactivate();
                hazardZone.ManuallyActivate();
                break;
            case HazardZoneTestState.PreAlarm:
                if (hazardZone.IsActive)
                    hazardZone.ManuallyDeactivate();
                hazardZone.ManuallyActivate();

                var personId = Guid.NewGuid();
                var insideLocation = new Location(2, 2);
                DomainEvents.Raise(new PersonCreatedEvent(personId, insideLocation));
                break;
        }

        return hazardZone;
    }

    private HazardZoneBuilder()
    {
    }
}
