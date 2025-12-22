using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain.States;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
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
    private readonly IPersonEvents _personEvents;

    public string Name { get; }
    public Outline Outline { get; }
    public TimeSpan PreAlarmDuration { get; }
    public bool IsActive => _currentState.IsActive;
    public AlarmState AlarmState => _currentState.AlarmState;
    public int AllowedNumberOfPersons => _currentState.AllowedNumberOfPersons;

    public event EventHandler<DomainEventArgs<PersonAddedToHazardZoneEvent>>? PersonAddedToHazardZone;
    public event EventHandler<DomainEventArgs<PersonRemovedFromHazardZoneEvent>>? PersonRemovedFromHazardZone;

    internal IClock Clock => _clock;
    internal ITimerFactory TimerFactory => _timerFactory;

    public HazardZone(string name, Outline outline, TimeSpan preAlarmDuration, IPersonEvents personEvents)
        : this(name, outline, preAlarmDuration, new SystemClock(), new SystemTimerFactory(), personEvents)
    {
    }

    public HazardZone(string name, Outline outline, TimeSpan preAlarmDuration, IClock clock, ITimerFactory timerFactory,
        IPersonEvents personEvents)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(timerFactory);
        ArgumentNullException.ThrowIfNull(personEvents);

        Name = name;
        Outline = outline;
        PreAlarmDuration = preAlarmDuration;

        _clock = clock;
        _timerFactory = timerFactory;
        _personEvents = personEvents;

        _currentState = new InactiveHazardZoneState(this, [], [], 0);

        _personEvents.Created += OnPersonCreatedEvent;
        _personEvents.Expired += OnPersonExpiredEvent;
        _personEvents.LocationChanged += OnPersonLocationChangedEvent;
    }

    public void Handle(PersonCreatedEvent personCreatedEvent)
    {
        ArgumentNullException.ThrowIfNull(personCreatedEvent);
        HandlePersonCreatedEvent(personCreatedEvent);
    }

    public void Handle(PersonExpiredEvent personExpiredEvent)
    {
        ArgumentNullException.ThrowIfNull(personExpiredEvent);
        HandlePersonExpiredEvent(personExpiredEvent);
    }

    public void Handle(PersonLocationChangedEvent personLocationChangedEvent)
    {
        ArgumentNullException.ThrowIfNull(personLocationChangedEvent);
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

    private void OnPersonCreatedEvent(object? _, DomainEventArgs<PersonCreatedEvent> args)
    {
        HandlePersonCreatedEvent(args.DomainEvent);
    }

    private void OnPersonExpiredEvent(object? _, DomainEventArgs<PersonExpiredEvent> args)
    {
        HandlePersonExpiredEvent(args.DomainEvent);
    }

    private void OnPersonLocationChangedEvent(object? _, DomainEventArgs<PersonLocationChangedEvent> args)
    {
        HandlePersonLocationChangedEvent(args.DomainEvent);
    }

    private void HandlePersonCreatedEvent(PersonCreatedEvent personCreatedEvent)
    {
        lock (_zoneStateLock)
        {
            if (!Outline.IsLocationInside(personCreatedEvent.Location))
                return;

            _currentState.OnPersonAddedToHazardZone(personCreatedEvent.PersonId);
        }
    }

    private void HandlePersonExpiredEvent(PersonExpiredEvent personExpiredEvent)
    {
        lock (_zoneStateLock) _currentState.OnPersonRemovedFromHazardZone(personExpiredEvent.PersonId);
    }

    private void HandlePersonLocationChangedEvent(PersonLocationChangedEvent personLocationChangedEvent)
    {
        lock (_zoneStateLock)
            _currentState.OnPersonChangedLocation(personLocationChangedEvent.PersonId,
                personLocationChangedEvent.CurrentLocation);
    }

    internal void RaisePersonAddedToHazardZone(Guid personId)
    {
        var handlers = PersonAddedToHazardZone;
        handlers?.Invoke(this,
            new DomainEventArgs<PersonAddedToHazardZoneEvent>(new PersonAddedToHazardZoneEvent(personId, Name)));
    }

    internal void RaisePersonRemovedFromHazardZone(Guid personId)
    {
        var handlers = PersonRemovedFromHazardZone;
        handlers?.Invoke(this,
            new DomainEventArgs<PersonRemovedFromHazardZoneEvent>(new PersonRemovedFromHazardZoneEvent(personId, Name)));
    }

    public void Dispose()
    {
        _personEvents.Created -= OnPersonCreatedEvent;
        _personEvents.Expired -= OnPersonExpiredEvent;
        _personEvents.LocationChanged -= OnPersonLocationChangedEvent;

        _currentState.Dispose();
    }
}

