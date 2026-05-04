using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain.States;
using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Events;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.Time;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public sealed class HazardZone : IDisposable
{
    private readonly Lock _zoneStateLock = new();
    private HazardZoneStateBase _currentState;

    public HazardZoneName Name { get; }
    public Outline Outline { get; }
    public Duration ActivationDuration { get; }
    public Duration PreAlarmDuration { get; }
    public ZoneState ZoneState => _currentState.ZoneState;
    public AlarmState AlarmState => _currentState.AlarmState;
    public Capacity AllowedNumberOfPersons => _currentState.AllowedNumberOfPersons;

    public event EventHandler<PersonAddedToHazardZoneEventArgs>? PersonAddedToHazardZone;
    public event EventHandler<PersonRemovedFromHazardZoneEventArgs>? PersonRemovedFromHazardZone;
    public event EventHandler<HazardZoneStateChangedEventArgs>? HazardZoneStateChanged;
    public event EventHandler<HazardZoneAlarmStateChangedEventArgs>? HazardZoneAlarmStateChanged;

    internal IClock Clock { get; }

    internal ITimerFactory TimerFactory { get; }

    public HazardZone(HazardZoneName name, Outline outline, Duration preAlarmDuration)
        : this(name, outline, Duration.From(TimeSpan.Zero), preAlarmDuration, new SystemClock(), new SystemTimerFactory())
    {
    }

    public HazardZone(HazardZoneName name, Outline outline, Duration activationDuration, Duration preAlarmDuration)
        : this(name, outline, activationDuration, preAlarmDuration, new SystemClock(), new SystemTimerFactory())
    {
    }

    public HazardZone(HazardZoneName name, Outline outline, Duration activationDuration, Duration preAlarmDuration, IClock clock, ITimerFactory timerFactory)
    {
        Guard.Against.Null(name);
        Guard.Against.Null(outline);
        Guard.Against.Null(activationDuration);
        Guard.Against.Null(preAlarmDuration);
        Guard.Against.Null(clock);
        Guard.Against.Null(timerFactory);

        Name = name;
        Outline = outline;
        ActivationDuration = activationDuration;
        PreAlarmDuration = preAlarmDuration;

        Clock = clock;
        TimerFactory = timerFactory;

        _currentState = new InactiveHazardZoneState(this, [], [], Capacity.From(0));
    }

    public void HandlePersonCreated(PersonId personId, Coordinate location)
    {
        Guard.Against.Null(location);

        lock (_zoneStateLock)
        {
            if (!Outline.IsLocationInside(location))
                return;

            _currentState.OnPersonAddedToHazardZone(personId);
        }
    }

    public void HandlePersonExpired(PersonId personId)
    {
        lock (_zoneStateLock)
        {
            _currentState.OnPersonRemovedFromHazardZone(personId);
        }
    }

    public void HandlePersonLocationChanged(PersonId personId, Coordinate location)
    {
        lock (_zoneStateLock)
        {
            _currentState.OnPersonChangedLocation(personId, location);
        }
    }

    public void ManuallyActivate()
    {
        lock (_zoneStateLock) _currentState.ManuallyActivate();
    }

    public void ManuallyDeactivate()
    {
        lock (_zoneStateLock) _currentState.ManuallyDeactivate();
    }

    public void ActivateFromExternalSource(SourceId sourceId)
    {
        Guard.Against.Null(sourceId);

        lock (_zoneStateLock) _currentState.ActivateFromExternalSource(sourceId);
    }

    public void DeactivateFromExternalSource(SourceId sourceId)
    {
        Guard.Against.Null(sourceId);

        lock (_zoneStateLock) _currentState.DeactivateFromExternalSource(sourceId);
    }

    public void SetAllowedNumberOfPersons(Capacity allowedNumberOfPersons)
    {
        Guard.Against.Null(allowedNumberOfPersons);

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

    internal void RaisePersonAddedToHazardZone(PersonId personId)
    {
        PersonAddedToHazardZone?.Invoke(this, new PersonAddedToHazardZoneEventArgs(personId, Name));
    }

    internal void RaisePersonRemovedFromHazardZone(PersonId personId)
    {
        PersonRemovedFromHazardZone?.Invoke(this, new PersonRemovedFromHazardZoneEventArgs(personId, Name));
    }

    internal void RaiseHazardZoneStateChanged(ZoneState newState)
    {
        HazardZoneStateChanged?.Invoke(this, new HazardZoneStateChangedEventArgs(Name, newState));
    }

    internal void RaiseHazardZoneAlarmStateChanged(AlarmState newState)
    {
        HazardZoneAlarmStateChanged?.Invoke(this, new HazardZoneAlarmStateChangedEventArgs(Name, newState));
    }

    public void Dispose()
    {
        _currentState.Dispose();
    }
}
