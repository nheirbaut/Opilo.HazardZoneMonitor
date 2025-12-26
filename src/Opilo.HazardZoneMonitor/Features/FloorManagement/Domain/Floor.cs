using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.Shared.Time;

namespace Opilo.HazardZoneMonitor.Features.FloorManagement.Domain;

public sealed class Floor : IDisposable
{
    private readonly Dictionary<Guid, Person> _personsOnFloor = [];
    private readonly TimeSpan _personLifespan;
    private volatile bool _disposed;
    private readonly Lock _personsOnFloorLock = new();
    private readonly IClock _clock;
    private readonly ITimerFactory _timerFactory;

    public readonly static TimeSpan DefaultPersonLifespan = TimeSpan.FromMilliseconds(200);

    public string Name { get; }
    public Outline Outline { get; }

    public event EventHandler<PersonAddedToFloorEventArgs>? PersonAddedToFloor;
    public event EventHandler<PersonRemovedFromFloorEventArgs>? PersonRemovedFromFloor;

    public Floor(string name, Outline outline, TimeSpan? personLifespan = null,
        IClock? clock = null,
        ITimerFactory? timerFactory = null)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);

        Name = name;
        Outline = outline;
        _personLifespan = personLifespan ?? DefaultPersonLifespan;
        _clock = clock ?? new SystemClock();
        _timerFactory = timerFactory ?? new SystemTimerFactory();
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
                    RemovePersonFromFloor(personLocationUpdate.PersonId);

                return false;
            }

            if (_personsOnFloor.TryGetValue(personLocationUpdate.PersonId, out var person))
            {
                person.UpdateLocation(personLocationUpdate.Location);
                return true;
            }

            var newPerson = Person.Create(personLocationUpdate.PersonId, personLocationUpdate.Location, _personLifespan,
                _clock, _timerFactory);

            newPerson.Expired += OnPersonExpired;
            _personsOnFloor.Add(personLocationUpdate.PersonId, newPerson);
        }

        PersonAddedToFloor?.Invoke(this,
            new PersonAddedToFloorEventArgs(Name, personLocationUpdate.PersonId, personLocationUpdate.Location));

        return true;
    }

    private void OnPersonExpired(object? _, PersonExpiredEventArgs args)
    {
        RemovePersonFromFloor(args.PersonId);
    }

    private void RemovePersonFromFloor(Guid personId)
    {
        Person? removedPerson;

        lock (_personsOnFloorLock)
        {
            if (!_personsOnFloor.Remove(personId, out removedPerson))
                return;

            PersonRemovedFromFloor?.Invoke(this, new PersonRemovedFromFloorEventArgs(Name, personId));
        }

        removedPerson.Expired -= OnPersonExpired;
        removedPerson.Dispose();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        List<Person>? persons;
        lock (_personsOnFloorLock)
        {
            persons = _personsOnFloor.Values.ToList();
            _personsOnFloor.Clear();
        }

        foreach (var person in persons)
        {
            person.Expired -= OnPersonExpired;
            person.Dispose();
        }
    }
}
