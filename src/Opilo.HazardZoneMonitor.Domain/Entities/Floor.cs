using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

public sealed class Floor
{
    private readonly HashSet<Guid> _personsOnFloor = [];

    public string Name { get; }
    public Outline Outline { get; }

    public Floor(string name, Outline outline)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;
    }

    public bool TryAddPersonLocationUpdate(PersonLocationUpdate personLocationUpdate)
    {
        Guard.Against.Null(personLocationUpdate);
        var locationIsOnFloor = Outline.IsLocationInside(personLocationUpdate.Location);

        if (!locationIsOnFloor)
            return false;

        if (_personsOnFloor.Add(personLocationUpdate.PersonId))
            DomainEvents.Raise(new PersonAddedToFloorEvent(Name, personLocationUpdate.PersonId, personLocationUpdate.Location));

        return true;
    }
}
