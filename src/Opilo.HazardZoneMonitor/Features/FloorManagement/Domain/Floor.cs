using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Guards;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.Shared.Time;

namespace Opilo.HazardZoneMonitor.Features.FloorManagement.Domain;

public sealed class Floor : IDisposable
{
    private readonly List<Person> _personsOnFloor = [];
    private readonly List<HazardZone> _hazardZones;
    private readonly TimeSpan _personLifespan;
    private volatile bool _disposed;
    private readonly Lock _personsOnFloorLock = new();
    private readonly IClock _clock;
    private readonly ITimerFactory _timerFactory;

    public readonly static TimeSpan DefaultPersonLifespan = TimeSpan.FromMilliseconds(200);

    public string Name { get; }
    public Outline Outline { get; }
    public IReadOnlyCollection<HazardZone> HazardZones => _hazardZones.AsReadOnly();

    public event EventHandler<PersonAddedToFloorEventArgs>? PersonAddedToFloor;
    public event EventHandler<PersonRemovedFromFloorEventArgs>? PersonRemovedFromFloor;

    public Floor(
        string name,
        Outline outline,
        IEnumerable<HazardZone> hazardZones,
        TimeSpan? personLifespan = null,
        IClock? clock = null,
        ITimerFactory? timerFactory = null)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);
        Guard.Against.Null(hazardZones);

        var hazardZoneList = hazardZones.ToList();
        Guard.Against.DuplicateHazardZones(hazardZoneList, nameof(hazardZones));
        Guard.Against.OverlappingHazardZones(hazardZoneList, nameof(hazardZones));
        Guard.Against.HazardZonesOutsideFloor(hazardZoneList, outline, nameof(hazardZones));

        Name = name;
        Outline = outline;
        _hazardZones = hazardZoneList;
        _personLifespan = personLifespan ?? DefaultPersonLifespan;
        _clock = clock ?? new SystemClock();
        _timerFactory = timerFactory ?? new SystemTimerFactory();
    }

    public bool TryAddPersonLocationUpdate(PersonLocationUpdate personLocationUpdate)
    {
        Guard.Against.Null(personLocationUpdate);

        var locationIsOnFloor = Outline.IsLocationInside(personLocationUpdate.Location);

        if (!locationIsOnFloor)
        {
            RemovePersonFromFloorIfPersonIsOnFloor(personLocationUpdate.PersonId);
            return false;
        }

        lock (_personsOnFloorLock)
        {
            if (_personsOnFloor.Any(p => p.TryLocationUpdate(personLocationUpdate)))
            {
                return true;
            }

            var newPerson = Person.Create(personLocationUpdate.PersonId, personLocationUpdate.Location, _personLifespan,
                _clock, _timerFactory);

            newPerson.Expired += OnPersonExpired;
            _personsOnFloor.Add(newPerson);

            PersonAddedToFloor?.Invoke(this,
                new PersonAddedToFloorEventArgs(Name, personLocationUpdate.PersonId, personLocationUpdate.Location));

            NotifyHazardZonesOfPersonCreated(personLocationUpdate.PersonId, personLocationUpdate.Location);
        }

        return true;
    }

    private void NotifyHazardZonesOfPersonCreated(Guid personId, Location location)
    {
        var personCreatedEvent = new PersonCreatedEventArgs(personId, location);
        foreach (var hazardZone in _hazardZones)
        {
            hazardZone.Handle(personCreatedEvent);
        }
    }

    private void OnPersonExpired(object? _, PersonExpiredEventArgs args)
    {
        RemovePersonFromFloorIfPersonIsOnFloor(args.PersonId);
    }

    private void RemovePersonFromFloorIfPersonIsOnFloor(Guid personId)
    {
        Person? personToRemove;

        lock (_personsOnFloorLock)
        {
            personToRemove = _personsOnFloor.FirstOrDefault(p => p.Id == personId);

            if (personToRemove == null)
                return;

            _personsOnFloor.Remove(personToRemove);

            PersonRemovedFromFloor?.Invoke(this, new PersonRemovedFromFloorEventArgs(Name, personId));
        }

        personToRemove.Expired -= OnPersonExpired;
        personToRemove.Dispose();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        List<Person>? persons;
        lock (_personsOnFloorLock)
        {
            persons = _personsOnFloor.ToList();
            _personsOnFloor.Clear();
        }

        foreach (var person in persons)
        {
            person.Expired -= OnPersonExpired;
            person.Dispose();
        }
    }
}
