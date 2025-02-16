using System.Diagnostics.CodeAnalysis;
using System.Timers;
using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Enums;
using Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Timer = System.Timers.Timer;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

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

    public HazardZone(string name, Outline outline, TimeSpan preAlarmDuration)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;
        PreAlarmDuration = preAlarmDuration;

        _currentState = new InactiveHazardZoneState(this, [], [], 0);

        DomainEvents.Register<PersonCreatedEvent>(OnPersonCreatedEvent);
        DomainEvents.Register<PersonExpiredEvent>(OnPersonExpiredEvent);
        DomainEvents.Register<PersonLocationChangedEvent>(OnPersonLocationChangedEvent);
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

internal abstract class HazardZoneStateBase(
    HazardZone hazardZone,
    HashSet<Guid> personsInZone,
    HashSet<string> registeredActivationSourceIds,
    int allowedNumberOfPersons) : IDisposable
{
    public abstract bool IsActive { get; }
    public abstract AlarmState AlarmState { get; }
    public int AllowedNumberOfPersons { get; private set; } = allowedNumberOfPersons;

    protected HazardZone HazardZone => hazardZone;
    protected HashSet<Guid> PersonsInZone => personsInZone;
    protected readonly HashSet<string> RegisteredActivationSourceIds = registeredActivationSourceIds;

    public void SetAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        AllowedNumberOfPersons = allowedNumberOfPersons;
        OnAllowedNumberOfPersonsChanged();
    }

    public void OnPersonAddedToHazardZone(Guid personId)
    {
        if (PersonsInZone.Add(personId))
            DomainEvents.Raise(new PersonAddedToHazardZoneEvent(personId, HazardZone.Name));

        OnPersonAddedToHazardZone();
    }

    public void OnPersonRemovedFromHazardZone(Guid personId)
    {
        if (PersonsInZone.Remove(personId))
            DomainEvents.Raise(new PersonRemovedFromHazardZoneEvent(personId, HazardZone.Name));

        OnPersonRemovedFromHazardZone();
    }

    public void OnPersonChangedLocation(Guid personId, Location location)
    {
        if (PersonsInZone.Contains(personId))
        {
            if (HazardZone.Outline.IsLocationInside(location))
                return;

            OnPersonRemovedFromHazardZone(personId);
        }

        if (!HazardZone.Outline.IsLocationInside(location))
            return;

        OnPersonAddedToHazardZone(personId);
    }

    protected virtual void OnPersonAddedToHazardZone()
    {
    }

    protected virtual void OnPersonRemovedFromHazardZone()
    {
    }

    public virtual void ManuallyActivate()
    {
    }

    public virtual void ManuallyDeactivate()
    {
    }

    public virtual void ActivateFromExternalSource(string sourceId)
    {
    }

    public virtual void DeactivateFromExternalSource(string sourceId)
    {
    }

    public virtual void OnPreAlarmTimerElapsed()
    {
    }

    protected virtual void OnAllowedNumberOfPersonsChanged()
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    ~HazardZoneStateBase()
    {
        Dispose(false);
    }
}

internal sealed class InactiveHazardZoneState(
    HazardZone hazardZone,
    HashSet<Guid> personsInZone,
    HashSet<string> registeredActivationSourceIds,
    int allowedNumberOfPersons)
    : HazardZoneStateBase(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
{
    public override bool IsActive => false;
    public override AlarmState AlarmState => AlarmState.None;

    public override void ManuallyActivate()
    {
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    public override void ActivateFromExternalSource(string sourceId)
    {
        if (!RegisteredActivationSourceIds.Add(sourceId))
            return;

        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }
}

internal sealed class ActiveHazardZoneState(
    HazardZone hazardZone,
    HashSet<Guid> personsInZone,
    HashSet<string> registeredActivationSourceIds,
    int allowedNumberOfPersons)
    : HazardZoneStateBase(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
{
    public override bool IsActive => true;
    public override AlarmState AlarmState => AlarmState.None;

    public override void ManuallyDeactivate()
    {
        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    public override void DeactivateFromExternalSource(string sourceId)
    {
        if (!RegisteredActivationSourceIds.Remove(sourceId))
            return;

        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnPersonAddedToHazardZone()
    {
        if (PersonsInZone.Count <= AllowedNumberOfPersons)
            return;

        HazardZone.TransitionTo(new PreAlarmHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }
}

internal sealed class PreAlarmHazardZoneState : HazardZoneStateBase
{
    private readonly Timer _preAlarmTimer;

    public PreAlarmHazardZoneState(HazardZone hazardZone, HashSet<Guid> personsInZone,
        HashSet<string> registeredActivationSourceIds, int allowedNumberOfPersons) :
        base(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
    {
        _preAlarmTimer = new Timer(HazardZone.PreAlarmDuration);
        _preAlarmTimer.Elapsed += OnPreAlarmTimerElapsed;
        _preAlarmTimer.Start();
    }

    public override bool IsActive => true;
    public override AlarmState AlarmState => AlarmState.PreAlarm;

    public override void OnPreAlarmTimerElapsed()
    {
        HazardZone.TransitionTo(new AlarmHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnPersonRemovedFromHazardZone()
    {
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnAllowedNumberOfPersonsChanged()
    {
        if (PersonsInZone.Count <= AllowedNumberOfPersons)
            HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
                AllowedNumberOfPersons));
    }

    private void OnPreAlarmTimerElapsed(object? _, ElapsedEventArgs __)
    {
        HazardZone.OnPreAlarmTimerElapsed();
    }

    protected override void Dispose(bool disposing)
    {
        _preAlarmTimer.Stop();
        _preAlarmTimer.Elapsed -= OnPreAlarmTimerElapsed;
        _preAlarmTimer.Dispose();

        base.Dispose(disposing);
    }
}

internal sealed class AlarmHazardZoneState(
    HazardZone hazardZone,
    HashSet<Guid> personsInZone,
    HashSet<string> registeredActivationSourceIds,
    int allowedNumberOfPersons)
    : HazardZoneStateBase(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
{
    public override bool IsActive => true;
    public override AlarmState AlarmState => AlarmState.Alarm;
}
