using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain.States;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.Shared.Time;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public sealed class HazardZone : IDisposable
{
    private readonly Lock _zoneStateLock = new();
    private HazardZoneStateBase _currentState;

    public string Name { get; }
    public Outline Outline { get; }
    public TimeSpan PreAlarmDuration { get; }
    public bool IsActive => _currentState.IsActive;
    public AlarmState AlarmState => _currentState.AlarmState;
    public int AllowedNumberOfPersons => _currentState.AllowedNumberOfPersons;

    public event EventHandler<PersonAddedToHazardZoneEventArgs>? PersonAddedToHazardZone;
    public event EventHandler<PersonRemovedFromHazardZoneEventArgs>? PersonRemovedFromHazardZone;

    internal IClock Clock { get; }

    internal ITimerFactory TimerFactory { get; }

    public HazardZone(string name, Outline outline, TimeSpan preAlarmDuration)
        : this(name, outline, preAlarmDuration, new SystemClock(), new SystemTimerFactory())
    {
    }

    public HazardZone(string name, Outline outline, TimeSpan preAlarmDuration, IClock clock, ITimerFactory timerFactory)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);
        Guard.Against.Negative(preAlarmDuration);
        Guard.Against.Null(clock);
        Guard.Against.Null(timerFactory);

        Name = name;
        Outline = outline;
        PreAlarmDuration = preAlarmDuration;

        Clock = clock;
        TimerFactory = timerFactory;

        _currentState = new InactiveHazardZoneState(this, [], [], 0);
    }

    public void HandlePersonCreated(Guid personId, Location location)
    {
        Guard.Against.Null(location);

        lock (_zoneStateLock)
        {
            if (!Outline.IsLocationInside(location))
                return;

            _currentState.OnPersonAddedToHazardZone(personId);
        }
    }

    public void HandlePersonExpired(Guid personId)
    {
        lock (_zoneStateLock)
        {
            _currentState.OnPersonRemovedFromHazardZone(personId);
        }
    }

    public void Handle(PersonLocationChangedEventArgs personLocationChangedEvent)
    {
        Guard.Against.Null(personLocationChangedEvent);
        HandlePersonLocationChangedEvent(personLocationChangedEvent);
    }

    public void ManuallyActivate()
    {
        lock (_zoneStateLock) _currentState.ManuallyActivate();
    }

    public void ManuallyDeactivate()
    {
        lock (_zoneStateLock) _currentState.ManuallyDeactivate();
    }

    public void ActivateFromExternalSource(string sourceId)
    {
        Guard.Against.NullOrWhiteSpace(sourceId);

        lock (_zoneStateLock) _currentState.ActivateFromExternalSource(sourceId);
    }

    public void DeactivateFromExternalSource(string sourceId)
    {
        Guard.Against.NullOrWhiteSpace(sourceId);

        lock (_zoneStateLock) _currentState.DeactivateFromExternalSource(sourceId);
    }

    public void SetAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        if (allowedNumberOfPersons < 0)
            return;

        lock (_zoneStateLock) _currentState.SetAllowedNumberOfPersons(allowedNumberOfPersons);
    }

    internal void TransitionTo(HazardZoneStateBase newState)
    {
        var oldState = _currentState;
        _currentState = newState;
        oldState.Dispose();
    }

    internal void OnPreAlarmTimerElapsed()
    {
        lock (_zoneStateLock) _currentState.OnPreAlarmTimerElapsed();
    }

    private void HandlePersonLocationChangedEvent(PersonLocationChangedEventArgs personLocationChangedEvent)
    {
        lock (_zoneStateLock)
            _currentState.OnPersonChangedLocation(personLocationChangedEvent.PersonId,
                personLocationChangedEvent.CurrentLocation);
    }

    internal void RaisePersonAddedToHazardZone(Guid personId)
    {
        var handlers = PersonAddedToHazardZone;
        handlers?.Invoke(this, new PersonAddedToHazardZoneEventArgs(personId, Name));
    }

    internal void RaisePersonRemovedFromHazardZone(Guid personId)
    {
        var handlers = PersonRemovedFromHazardZone;
        handlers?.Invoke(this, new PersonRemovedFromHazardZoneEventArgs(personId, Name));
    }

    public void Dispose()
    {
        _currentState.Dispose();
    }
}

