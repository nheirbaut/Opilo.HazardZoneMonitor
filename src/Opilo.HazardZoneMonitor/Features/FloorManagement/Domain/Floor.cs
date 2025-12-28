using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
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
    private readonly ITimerFactory _timerFactory;

    public readonly static TimeSpan DefaultPersonLifespan = TimeSpan.FromMilliseconds(200);

    public string Name { get; }
    public Outline Outline { get; }
    public IReadOnlyCollection<HazardZone> HazardZones => _hazardZones.AsReadOnly();

    public event EventHandler<PersonAddedToFloorEventArgs>? PersonAddedToFloor;
    public event EventHandler<PersonRemovedFromFloorEventArgs>? PersonRemovedFromFloor;
    public event EventHandler<PersonAddedToHazardZoneEventArgs>? PersonAddedToHazardZone;
    public event EventHandler<PersonRemovedFromHazardZoneEventArgs>? PersonRemovedFromHazardZone;

    public Floor(
        string name,
        Outline outline,
        IList<HazardZone> hazardZones,
        TimeSpan? personLifespan = null,
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
        _timerFactory = timerFactory ?? new SystemTimerFactory();

        foreach (var hazardZone in _hazardZones)
        {
            hazardZone.PersonAddedToHazardZone += OnHazardZonePersonAdded;
            hazardZone.PersonRemovedFromHazardZone += OnHazardZonePersonRemoved;
        }
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

        bool isNewPerson;
        Guid personId;
        Location location;

        lock (_personsOnFloorLock)
        {
            if (_personsOnFloor.Any(p => p.TryLocationUpdate(personLocationUpdate)))
            {
                return true;
            }

            var newPerson = Person.Create(personLocationUpdate.PersonId, personLocationUpdate.Location, _personLifespan,
                _timerFactory);

            newPerson.Expired += OnPersonExpired;
            newPerson.LocationChanged += OnPersonLocationChanged;
            _personsOnFloor.Add(newPerson);

            isNewPerson = true;
            personId = personLocationUpdate.PersonId;
            location = personLocationUpdate.Location;
        }

        if (isNewPerson)
        {
            PersonAddedToFloor?.Invoke(this, new PersonAddedToFloorEventArgs(Name, personId, location));
            NotifyHazardZonesOfPersonCreated(personId, location);
        }

        return true;
    }

    private void NotifyHazardZonesOfPersonCreated(Guid personId, Location location)
    {
        if (_disposed)
            return;

        foreach (var hazardZone in _hazardZones)
        {
            hazardZone.HandlePersonCreated(personId, location);
        }
    }

    private void OnPersonExpired(object? _, PersonExpiredEventArgs args)
    {
        if (!_disposed)
        {
            foreach (var hazardZone in _hazardZones)
            {
                hazardZone.HandlePersonExpired(args.PersonId);
            }
        }

        RemovePersonFromFloorIfPersonIsOnFloor(args.PersonId);
    }

    private void OnPersonLocationChanged(object? _, PersonLocationChangedEventArgs args)
    {
        if (_disposed)
            return;

        foreach (var hazardZone in _hazardZones)
        {
            hazardZone.HandlePersonLocationChanged(args.PersonId, args.CurrentLocation);
        }
    }

    private void OnHazardZonePersonAdded(object? _, PersonAddedToHazardZoneEventArgs args)
    {
        PersonAddedToHazardZone?.Invoke(this, args);
    }

    private void OnHazardZonePersonRemoved(object? _, PersonRemovedFromHazardZoneEventArgs args)
    {
        PersonRemovedFromHazardZone?.Invoke(this, args);
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
        personToRemove.LocationChanged -= OnPersonLocationChanged;
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
            person.LocationChanged -= OnPersonLocationChanged;
            person.Dispose();
        }

        foreach (var hazardZone in _hazardZones)
        {
            hazardZone.PersonAddedToHazardZone -= OnHazardZonePersonAdded;
            hazardZone.PersonRemovedFromHazardZone -= OnHazardZonePersonRemoved;
        }
    }
}
