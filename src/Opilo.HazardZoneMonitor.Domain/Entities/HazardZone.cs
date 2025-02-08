using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Enums;
using Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public sealed class HazardZone : IDisposable
{
    private readonly HashSet<Guid> _personsInZone = [];
    private readonly Lock _personsInZoneLock = new();
    private readonly Lock _zoneStateLock = new();
    private HazardZoneStateBase _currentState;
    private readonly HashSet<string> _registeredActivationSourceIds = [];
    private int _maximumAllowedNumberOfPersons;

    public string Name { get; }
    public Outline Outline { get; }
    public bool IsActive => _currentState.IsActive;
    public AlarmState AlarmState => _currentState.AlarmState;
    public bool MorePersonsThanAllowed => _personsInZone.Count > _maximumAllowedNumberOfPersons;

    public HazardZone(string name, Outline outline)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;

        _currentState = new InactiveHazardZoneState(this);

        DomainEvents.Register<PersonCreatedEvent>(OnPersonCreatedEvent);
        DomainEvents.Register<PersonExpiredEvent>(OnPersonExpiredEvent);
        DomainEvents.Register<PersonLocationChangedEvent>(OnPersonLocationChangedEvent);
    }

    public void ManuallyActivate()
    {
        lock (_zoneStateLock)
        {
            _currentState.ManuallyActivate();
        }
    }

    public void ManuallyDeactivate()
    {
        lock (_zoneStateLock)
        {
            _currentState.ManuallyDeactivate();
        }
    }

    public void ActivateFromExternalSource(string sourceId)
    {
        Guard.Against.NullOrWhiteSpace(sourceId);

        lock (_zoneStateLock)
        {
            _currentState.ActivateFromExternalSource(sourceId);
        }
    }

    public void DeactivateFromExternalSource(string sourceId)
    {
        Guard.Against.NullOrWhiteSpace(sourceId);

        lock (_zoneStateLock)
        {
            _currentState.DeactivateFromExternalSource(sourceId);
        }
    }

    public void SetAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        if (allowedNumberOfPersons < 0)
            return;

        lock (_zoneStateLock)
        {
            _maximumAllowedNumberOfPersons = allowedNumberOfPersons;
        }
    }

    internal void TransitionTo(HazardZoneStateBase newState)
    {
        var oldState = _currentState;
        _currentState = newState;
        oldState.Dispose();
    }

    internal bool RegisterActivationSourceId(string sourceId) => _registeredActivationSourceIds.Add(sourceId);
    internal bool UnregisterActivationSourceId(string sourceId) => _registeredActivationSourceIds.Remove(sourceId);

    private void OnPersonCreatedEvent(PersonCreatedEvent personCreatedEvent)
    {
        lock (_personsInZoneLock)
        {
            if (!Outline.IsLocationInside(personCreatedEvent.Location))
                return;

            AddPerson(personCreatedEvent.PersonId);
        }
    }

    private void OnPersonExpiredEvent(PersonExpiredEvent personExpiredEvent)
    {
        lock (_personsInZoneLock)
        {
            if (!_personsInZone.Contains(personExpiredEvent.PersonId))
                return;

            RemovePerson(personExpiredEvent.PersonId);
        }
    }

    private void OnPersonLocationChangedEvent(PersonLocationChangedEvent personLocationChangedEvent)
    {
        lock (_personsInZoneLock)
        {
            if (_personsInZone.Contains(personLocationChangedEvent.PersonId))
            {
                if (Outline.IsLocationInside(personLocationChangedEvent.CurrentLocation))
                    return;

                RemovePerson(personLocationChangedEvent.PersonId);
                return;
            }

            if (!Outline.IsLocationInside(personLocationChangedEvent.CurrentLocation))
                return;

            AddPerson(personLocationChangedEvent.PersonId);
        }
    }

    private void AddPerson(Guid personId)
    {
        _personsInZone.Add(personId);
        DomainEvents.Raise(new PersonAddedToHazardZoneEvent(personId, Name));

        _currentState.OnPersonAddedToHazardZone();
    }

    private void RemovePerson(Guid personId)
    {
        _personsInZone.Remove(personId);
        DomainEvents.Raise(new PersonRemovedFromHazardZoneEvent(personId, Name));

        _currentState.OnPersonRemovedFromHazardZone();
    }

    public void Dispose()
    {
        _currentState.Dispose();
    }
}

internal abstract class HazardZoneStateBase(HazardZone hazardZone) : IDisposable
{
    public abstract bool IsActive { get; }
    public abstract AlarmState AlarmState { get; }

    protected HazardZone HazardZone { get; } = hazardZone;

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

    public virtual void OnPersonAddedToHazardZone()
    {
    }

    public virtual void OnPersonRemovedFromHazardZone()
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

internal sealed class InactiveHazardZoneState(HazardZone hazardZone) : HazardZoneStateBase(hazardZone)
{
    public override bool IsActive => false;
    public override AlarmState AlarmState => AlarmState.None;

    public override void ManuallyActivate()
    {
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone));
    }

    public override void ActivateFromExternalSource(string sourceId)
    {
        if (!HazardZone.RegisterActivationSourceId(sourceId))
            return;

        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone));
    }
}

internal sealed class ActiveHazardZoneState(HazardZone hazardZone) : HazardZoneStateBase(hazardZone)
{
    public override bool IsActive => true;
    public override AlarmState AlarmState => AlarmState.None;

    public override void ManuallyDeactivate()
    {
        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone));
    }

    public override void OnPersonAddedToHazardZone()
    {
        HazardZone.TransitionTo(new PreAlarmHazardZoneState(HazardZone));
    }

    public override void DeactivateFromExternalSource(string sourceId)
    {
        if (!HazardZone.UnregisterActivationSourceId(sourceId))
            return;

        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone));
    }
}

internal sealed class PreAlarmHazardZoneState(HazardZone hazardZone) : HazardZoneStateBase(hazardZone)
{
    public override bool IsActive => true;
    public override AlarmState AlarmState => AlarmState.PreAlarm;

    public override void OnPersonRemovedFromHazardZone()
    {
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone));
    }
}
