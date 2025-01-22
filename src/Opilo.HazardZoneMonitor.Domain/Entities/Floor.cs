using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Events.FloorEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

public sealed class Floor : IDisposable
{
    private readonly Dictionary<Guid, Person> _personsOnFloor = [];
    private readonly TimeSpan _personLifespan;
    private volatile bool _disposed;
    private readonly Lock _personsOnFloorLock = new();

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
        RemovePersonFromFloorAndRaisePersonRemovedFromFloorEvent(personExpiredEvent.PersonId);
    }

    public bool TryAddPersonLocationUpdate(PersonLocationUpdate personLocationUpdate)
    {
        Guard.Against.Null(personLocationUpdate);

        var locationIsOnFloor = Outline.IsLocationInside(personLocationUpdate.Location);

        lock (_personsOnFloorLock)
        {
            if (!locationIsOnFloor)
            {
                if (_personsOnFloor.ContainsKey(personLocationUpdate.PersonId))
                    RemovePersonFromFloorAndRaisePersonRemovedFromFloorEvent(personLocationUpdate.PersonId);

                return false;
            }

            if (_personsOnFloor.TryGetValue(personLocationUpdate.PersonId, out var person))
            {
                person.UpdateLocation(personLocationUpdate.Location);
                return true;
            }

            _personsOnFloor.Add(
                personLocationUpdate.PersonId,
                Person.Create(personLocationUpdate.PersonId, personLocationUpdate.Location, _personLifespan));
        }

        DomainEvents.Raise(new PersonAddedToFloorEvent(Name, personLocationUpdate.PersonId,
            personLocationUpdate.Location));

        return true;
    }

    private void RemovePersonFromFloorAndRaisePersonRemovedFromFloorEvent(Guid personId)
    {
        lock (_personsOnFloorLock)
        {
            if (_personsOnFloor.Remove(personId, out _))
                DomainEvents.Raise(new PersonRemovedFromFloorEvent(Name, personId));
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var (_, person) in _personsOnFloor)
            person.Dispose();

        _personsOnFloor.Clear();
    }
}
