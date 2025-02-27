using System.Collections.Concurrent;
using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;
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

    public IReadOnlyCollection<Guid> IdsOfPersonsAdded { get; private set; } = [];

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
                DeactivateIfActive(hazardZone);
                break;
            case HazardZoneTestState.Active:
                DeactivateIfActive(hazardZone);
                hazardZone.ManuallyActivate();
                break;
            case HazardZoneTestState.PreAlarm:
                ConfigurePreAlarmState(hazardZone);
                break;
        }

        return hazardZone;
    }

    private static void DeactivateIfActive(HazardZone hazardZone)
    {
        if (hazardZone.IsActive)
            hazardZone.ManuallyDeactivate();
    }

    private void ConfigurePreAlarmState(HazardZone hazardZone)
    {
        DeactivateIfActive(hazardZone);
        hazardZone.ManuallyActivate();

        var personsToAdd = _allowedNumberOfPersons + 1;
        var waiter = new EventCountWaiter(personsToAdd);

        DomainEvents.Register<PersonAddedToHazardZoneEvent>(e => waiter.Signal(e));

        foreach (var _ in Enumerable.Range(0, personsToAdd))
        {
            var personId = Guid.NewGuid();
            var insideLocation = new Location(2, 2);
            DomainEvents.Raise(new PersonCreatedEvent(personId, insideLocation));
        }

        IdsOfPersonsAdded = waiter.Wait(TimeSpan.FromSeconds(5)).Select(e => e.PersonId).ToList();
    }

    private HazardZoneBuilder()
    {
    }
}

internal sealed class EventCountWaiter(int expectedCount)
{
    private int _currentCount;
    private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // ReSharper disable once CollectionNeverQueried.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public ConcurrentBag<PersonAddedToHazardZoneEvent> ReceivedEvents { get;  } = [];

    public void Signal(PersonAddedToHazardZoneEvent personAddedToHazardZoneEvent)
    {
        ReceivedEvents.Add(personAddedToHazardZoneEvent);
        if (Interlocked.Increment(ref _currentCount) == expectedCount)
            _tcs.TrySetResult(true);
    }

    public List<PersonAddedToHazardZoneEvent> Wait(TimeSpan timeout)
    {
        if (!_tcs.Task.Wait(timeout))
            throw new TimeoutException(
                $"Timed out waiting for {expectedCount} events. Only {_currentCount} events were received.");

        return ReceivedEvents.ToList();
    }
}

