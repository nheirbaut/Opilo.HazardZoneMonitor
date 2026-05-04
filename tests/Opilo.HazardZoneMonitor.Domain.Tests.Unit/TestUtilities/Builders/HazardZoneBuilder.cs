using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities.Builders;

internal sealed class HazardZoneBuilder
{
    private HazardZoneTestState _desiredState = HazardZoneTestState.Inactive;
    private readonly List<SourceId> _externalActivationSourceIds = [];
    private Capacity _allowedNumberOfPersons = Capacity.From(0);
    private Duration _activationDuration = Duration.From(TimeSpan.Zero);
    private Duration _preAlarmDuration = DefaultPreAlarmDuration;
    private IClock? _clock;
    private ITimerFactory? _timerFactory;

    public static readonly HazardZoneName DefaultName = HazardZoneName.From("HazardZone");

    public static readonly Outline DefaultOutline = new(new([
        new Coordinate(0, 0),
        new Coordinate(4, 0),
        new Coordinate(4, 4),
        new Coordinate(0, 4)
    ]));

    public static readonly Duration DefaultPreAlarmDuration = Duration.From(TimeSpan.FromSeconds(5));

    public static HazardZone BuildSimple() => new(DefaultName, DefaultOutline, DefaultPreAlarmDuration);

    public IReadOnlyCollection<PersonId> IdsOfPersonsAdded { get; private set; } = [];

    public static HazardZoneBuilder Create() => new();

    public HazardZoneBuilder WithExternalActivationSource(string sourceId)
    {
        _externalActivationSourceIds.Add(SourceId.From(sourceId));
        return this;
    }

    public HazardZoneBuilder WithAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        _allowedNumberOfPersons = Capacity.From(allowedNumberOfPersons);
        return this;
    }

    public HazardZoneBuilder WithState(HazardZoneTestState state)
    {
        _desiredState = state;
        return this;
    }

    public HazardZoneBuilder WithActivationDuration(TimeSpan duration)
    {
        _activationDuration = Duration.From(duration);
        return this;
    }

    public HazardZoneBuilder WithPreAlarmDuration(TimeSpan duration)
    {
        _preAlarmDuration = Duration.From(duration);
        return this;
    }

    public HazardZoneBuilder WithTime(IClock clock, ITimerFactory timerFactory)
    {
        _clock = clock;
        _timerFactory = timerFactory;
        return this;
    }

    public HazardZone Build()
    {
        if (_desiredState == HazardZoneTestState.Alarm)
            _preAlarmDuration = Duration.From(TimeSpan.Zero);

        var hazardZone = (_clock is not null && _timerFactory is not null)
            ? new HazardZone(DefaultName, DefaultOutline, _activationDuration, _preAlarmDuration, _clock, _timerFactory)
            : new HazardZone(DefaultName, DefaultOutline, _activationDuration, _preAlarmDuration);
        hazardZone.SetAllowedNumberOfPersons(_allowedNumberOfPersons);

        foreach (var sourceId in _externalActivationSourceIds)
            hazardZone.ActivateFromExternalSource(sourceId);

        switch (_desiredState)
        {
            case HazardZoneTestState.Inactive:
                DeactivateIfActive(hazardZone);
                break;
            case HazardZoneTestState.Active:
                DeactivateIfActive(hazardZone);
                hazardZone.ManuallyActivate();
                break;
            case HazardZoneTestState.PreAlarm:
                ConfigurePreAlarmState(hazardZone);
                break;
            case HazardZoneTestState.Alarm:
                ConfigureAlarmState(hazardZone);
                break;
        }

        return hazardZone;
    }

    private static void DeactivateIfActive(HazardZone hazardZone)
    {
        if (hazardZone.ZoneState != ZoneState.Inactive)
            hazardZone.ManuallyDeactivate();
    }

    private void ConfigurePreAlarmState(HazardZone hazardZone)
    {
        DeactivateIfActive(hazardZone);
        hazardZone.ManuallyActivate();

        var personsToAdd = _allowedNumberOfPersons.Value + 1;
        var waiter = new EventCountWaiter(personsToAdd);

        hazardZone.PersonAddedToHazardZone += (_, e) => waiter.Signal(e);

        foreach (var _ in Enumerable.Range(0, personsToAdd))
        {
            var personId = PersonId.From(Guid.NewGuid());
            var insideLocation = new Coordinate(2, 2);
            hazardZone.HandlePersonCreated(personId, insideLocation);
        }

        IdsOfPersonsAdded = waiter.Wait(TimeSpan.FromSeconds(5)).Select(e => e.PersonId).ToList();
    }

    private void ConfigureAlarmState(HazardZone hazardZone)
    {
        ConfigurePreAlarmState(hazardZone);
    }

    private HazardZoneBuilder()
    {
    }
}
