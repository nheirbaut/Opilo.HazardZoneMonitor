using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

public sealed class HazardZone
{
    private readonly HashSet<Guid> _personInZone = [];
    private readonly Lock _personsInZoneLock = new();
    private readonly Lock _activationStateLock = new();
    private bool _zoneIsActivating;

    public string Name { get; }
    public Outline Outline { get; }

    public HazardZone(string name, Outline outline)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;

        DomainEvents.Register<PersonCreatedEvent>(OnPersonCreatedEvent);
        DomainEvents.Register<PersonExpiredEvent>(OnPersonExpiredEvent);
        DomainEvents.Register<PersonLocationChangedEvent>(OnPersonLocationChangedEvent);
    }

    public void Activate()
    {
        lock (_activationStateLock)
        {
            if (_zoneIsActivating)
                return;

            _zoneIsActivating = true;
            DomainEvents.Raise(new HazardZoneActivationStartedEvent(Name));
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
}
