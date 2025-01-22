using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

public sealed class HazardZone
{
    public string Name { get; }
    public Outline Outline { get; }

    public HazardZone(string name, Outline outline)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;

        DomainEvents.Register<PersonCreatedEvent>(OnPersonCreatedEvent);
    }

    private void OnPersonCreatedEvent(PersonCreatedEvent personCreatedEvent)
    {
        if (Outline.IsLocationInside(personCreatedEvent.Location))
            DomainEvents.Raise(new PersonAddedToHazardZoneEvent(personCreatedEvent.PersonId, Name));
    }
}
