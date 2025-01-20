using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

public sealed class Floor : IDisposable
{
    private readonly ConcurrentDictionary<Guid, Person> _personsOnFloor = [];
    private readonly TimeSpan _personLifespan;
    private volatile bool _disposed;

    public readonly static TimeSpan DefaultPersonLifespan = TimeSpan.FromMilliseconds(200);

    public string Name { get; }
    public Outline Outline { get; }

    public Floor(string name, Outline outline, TimeSpan? personLifespan = null)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;
        _personLifespan = personLifespan ?? DefaultPersonLifespan;

        DomainEvents.Register<PersonExpiredEvent>(OnPersonExpired);
    }

    private void OnPersonExpired(PersonExpiredEvent personExpiredEvent)
    {
        if (_personsOnFloor.Remove(personExpiredEvent.PersonId, out _))
            DomainEvents.Raise(new PersonRemovedFromFloorEvent(Name, personExpiredEvent.PersonId));
    }

    public bool TryAddPersonLocationUpdate(PersonLocationUpdate personLocationUpdate)
    {
        Guard.Against.Null(personLocationUpdate);
        var locationIsOnFloor = Outline.IsLocationInside(personLocationUpdate.Location);

        if (!locationIsOnFloor)
            return false;

        var personAdded = false;

        _personsOnFloor.AddOrUpdate(
            personLocationUpdate.PersonId,
            personId =>
            {
                personAdded = true;
                return Person.Create(personId, personLocationUpdate.Location, _personLifespan);
            },
            (_, person) =>
            {
                person.UpdateLocation(personLocationUpdate.Location);
                return person;
            });

        if (personAdded)
            DomainEvents.Raise(new PersonAddedToFloorEvent(Name, personLocationUpdate.PersonId, personLocationUpdate.Location));

        return true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _personsOnFloor.Clear();
    }
}
