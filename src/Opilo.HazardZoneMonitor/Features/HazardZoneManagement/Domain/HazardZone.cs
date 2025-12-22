using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain.States;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.Shared.Time;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public sealed class HazardZone : IDisposable
{
    private readonly Lock _zoneStateLock = new();
    private HazardZoneStateBase _currentState;
    private readonly IClock _clock;
    private readonly ITimerFactory _timerFactory;

    public string Name { get; }
    public Outline Outline { get; }
    public TimeSpan PreAlarmDuration { get; }
    public bool IsActive => _currentState.IsActive;
    public AlarmState AlarmState => _currentState.AlarmState;
    public int AllowedNumberOfPersons => _currentState.AllowedNumberOfPersons;

    internal IClock Clock => _clock;
    internal ITimerFactory TimerFactory => _timerFactory;

    public HazardZone(string name, Outline outline, TimeSpan preAlarmDuration)
        : this(name, outline, preAlarmDuration, new SystemClock(), new SystemTimerFactory())
    {
    }

    public HazardZone(string name, Outline outline, TimeSpan preAlarmDuration, IClock clock, ITimerFactory timerFactory)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;
        PreAlarmDuration = preAlarmDuration;

        _clock = clock;
        _timerFactory = timerFactory;

        _currentState = new InactiveHazardZoneState(this, [], [], 0);

        DomainEventDispatcher.Register<PersonCreatedEvent>(OnPersonCreatedEvent);
        DomainEventDispatcher.Register<PersonExpiredEvent>(OnPersonExpiredEvent);
        DomainEventDispatcher.Register<PersonLocationChangedEvent>(OnPersonLocationChangedEvent);
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

    private void OnPersonCreatedEvent(PersonCreatedEvent personCreatedEvent)
    {
        lock (_zoneStateLock)
        {
            if (!Outline.IsLocationInside(personCreatedEvent.Location))
                return;

            _currentState.OnPersonAddedToHazardZone(personCreatedEvent.PersonId);
        }
    }

    private void OnPersonExpiredEvent(PersonExpiredEvent personExpiredEvent)
    {
        lock (_zoneStateLock) _currentState.OnPersonRemovedFromHazardZone(personExpiredEvent.PersonId);
    }

    private void OnPersonLocationChangedEvent(PersonLocationChangedEvent personLocationChangedEvent)
    {
        lock (_zoneStateLock)
            _currentState.OnPersonChangedLocation(personLocationChangedEvent.PersonId,
                personLocationChangedEvent.CurrentLocation);
    }

    public void Dispose()
    {
        _currentState.Dispose();
    }
}

