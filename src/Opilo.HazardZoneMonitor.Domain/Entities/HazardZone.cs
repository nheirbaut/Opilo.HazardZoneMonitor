using System.Runtime.InteropServices;
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
    }

    private void OnPersonCreatedEvent(PersonCreatedEvent personCreatedEvent)
    {
        lock (_personsInZoneLock)
        {
            if (Outline.IsLocationInside(personCreatedEvent.Location))
            {
                _personInZone.Add(personCreatedEvent.PersonId);
                DomainEvents.Raise(new PersonAddedToHazardZoneEvent(personCreatedEvent.PersonId, Name));
            }
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
}
