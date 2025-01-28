using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Enums;
using Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public sealed class HazardZone
{
    private readonly HashSet<Guid> _personInZone = [];
    private readonly Lock _personsInZoneLock = new();
    private readonly Lock _zoneStateLock = new();
    private HazardZoneStateBase _currentState;

    public string Name { get; }
    public Outline Outline { get; }
    public bool IsActive { get; private set; }
    public AlarmState AlarmState { get; private set; }

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

    private void OnPersonCreatedEvent(PersonCreatedEvent personCreatedEvent)
    {
        lock (_personsInZoneLock)
        {
            if (!Outline.IsLocationInside(personCreatedEvent.Location))
                return;

            _personInZone.Add(personCreatedEvent.PersonId);
            DomainEvents.Raise(new PersonAddedToHazardZoneEvent(personCreatedEvent.PersonId, Name));
        }
    }

    private void OnPersonExpiredEvent(PersonExpiredEvent personExpiredEvent)
    {
        lock (_personsInZoneLock)
        {
            if (_personInZone.Remove(personExpiredEvent.PersonId))
                DomainEvents.Raise(new PersonRemovedFromHazardZoneEvent(personExpiredEvent.PersonId, Name));
        }
    }

    private void OnPersonLocationChangedEvent(PersonLocationChangedEvent personLocationChangedEvent)
    {
        lock (_personsInZoneLock)
        {
            if (_personInZone.Contains(personLocationChangedEvent.PersonId))
            {
                if (Outline.IsLocationInside(personLocationChangedEvent.CurrentLocation))
                    return;

                _personInZone.Remove(personLocationChangedEvent.PersonId);
                DomainEvents.Raise(new PersonRemovedFromHazardZoneEvent(personLocationChangedEvent.PersonId, Name));

                return;
            }

            if (!Outline.IsLocationInside(personLocationChangedEvent.CurrentLocation))
                return;

            _personInZone.Add(personLocationChangedEvent.PersonId);
            DomainEvents.Raise(new PersonAddedToHazardZoneEvent(personLocationChangedEvent.PersonId, Name));
        }
    }

    internal void TransitionTo(HazardZoneStateBase newState) => _currentState = newState;
    internal void SetIsActive(bool active) => IsActive = active;
    internal void SetAlarmState(AlarmState state) => AlarmState = state;
}

internal abstract class HazardZoneStateBase(HazardZone hazardZone)
{
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
}

internal sealed class InactiveHazardZoneState : HazardZoneStateBase
{
    public InactiveHazardZoneState(HazardZone hazardZone)
        : base(hazardZone)
    {
        hazardZone.SetIsActive(false);
        hazardZone.SetAlarmState(AlarmState.None);
    }

    public override void ManuallyActivate()
    {
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone));
    }

    public override void ActivateFromExternalSource(string sourceId)
    {
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone));
    }
}

internal sealed class ActiveHazardZoneState : HazardZoneStateBase
{
    public ActiveHazardZoneState(HazardZone hazardZone)
        : base(hazardZone)
    {
        hazardZone.SetIsActive(true);
        hazardZone.SetAlarmState(AlarmState.None);
    }

    public override void ManuallyDeactivate()
    {
        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone));
    }
}
