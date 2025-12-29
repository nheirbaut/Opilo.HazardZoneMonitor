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

    public string Name { get; }
    public Outline Outline { get; }
    public TimeSpan ActivationDuration { get; }
    public TimeSpan PreAlarmDuration { get; }
    public ZoneState ZoneState => _currentState.ZoneState;
    public AlarmState AlarmState => _currentState.AlarmState;
    public int AllowedNumberOfPersons => _currentState.AllowedNumberOfPersons;

    public event EventHandler<PersonAddedToHazardZoneEventArgs>? PersonAddedToHazardZone;
    public event EventHandler<PersonRemovedFromHazardZoneEventArgs>? PersonRemovedFromHazardZone;
    public event EventHandler<HazardZoneStateChangedEventArgs>? HazardZoneStateChanged;
    public event EventHandler<HazardZoneAlarmStateChangedEventArgs>? HazardZoneAlarmStateChanged;

    internal IClock Clock { get; }

    internal ITimerFactory TimerFactory { get; }

    public HazardZone(string name, Outline outline, TimeSpan preAlarmDuration)
        : this(name, outline, TimeSpan.Zero, preAlarmDuration, new SystemClock(), new SystemTimerFactory())
    {
    }

    public HazardZone(string name, Outline outline, TimeSpan activationDuration, TimeSpan preAlarmDuration)
        : this(name, outline, activationDuration, preAlarmDuration, new SystemClock(), new SystemTimerFactory())
    {
    }

    public HazardZone(string name, Outline outline, TimeSpan activationDuration, TimeSpan preAlarmDuration, IClock clock, ITimerFactory timerFactory)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);
        Guard.Against.Negative(activationDuration);
        Guard.Against.Negative(preAlarmDuration);
        Guard.Against.Null(clock);
        Guard.Against.Null(timerFactory);

        Name = name;
        Outline = outline;
        ActivationDuration = activationDuration;
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

    public void HandlePersonLocationChanged(Guid personId, Location location)
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

    internal void RaisePersonAddedToHazardZone(Guid personId)
    {
        PersonAddedToHazardZone?.Invoke(this, new PersonAddedToHazardZoneEventArgs(personId, Name));
    }

    internal void RaisePersonRemovedFromHazardZone(Guid personId)
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

