using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using TrackedPerson = Opilo.HazardZoneMonitor.Features.PersonTracking.Domain.Person;

namespace Opilo.HazardZoneMonitor.Features.FloorManagement.Domain;

public sealed class Floor : IDisposable
{
    private readonly Dictionary<Guid, TrackedPerson> _personsOnFloor = [];
    private readonly TimeSpan _personLifespan;
    private readonly IPersonEvents _personEvents;
    private volatile bool _disposed;
    private readonly Lock _personsOnFloorLock = new();

    public readonly static TimeSpan DefaultPersonLifespan = TimeSpan.FromMilliseconds(200);

    public string Name { get; }
    public Outline Outline { get; }

    public event EventHandler<PersonAddedToFloorEventArgs>? PersonAddedToFloor;
    public event EventHandler<PersonRemovedFromFloorEventArgs>? PersonRemovedFromFloor;

    public Floor(string name, Outline outline, IPersonEvents personEvents, TimeSpan? personLifespan = null)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(outline);
        ArgumentNullException.ThrowIfNull(personEvents);

        Name = name;
        Outline = outline;
        _personEvents = personEvents;
        _personLifespan = personLifespan ?? DefaultPersonLifespan;

        _personEvents.Expired += OnPersonExpired;
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

            _personsOnFloor.Add(
                personLocationUpdate.PersonId,
                TrackedPerson.Create(personLocationUpdate.PersonId, personLocationUpdate.Location, _personLifespan,
                    _personEvents));
        }

        var addedHandlers = PersonAddedToFloor;
        addedHandlers?.Invoke(this,
            new PersonAddedToFloorEventArgs(Name, personLocationUpdate.PersonId, personLocationUpdate.Location));

        return true;
    }

    private void OnPersonExpired(object? _, DomainEventArgs<PersonExpiredEvent> args)
    {
        RemovePersonFromFloor(args.DomainEvent.PersonId);
    }

    private void RemovePersonFromFloor(Guid personId)
    {
        lock (_personsOnFloorLock)
        {
            if (_personsOnFloor.Remove(personId, out _))
            {
                var removedHandlers = PersonRemovedFromFloor;
                removedHandlers?.Invoke(this,
                    new PersonRemovedFromFloorEventArgs(Name, personId));
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _personEvents.Expired -= OnPersonExpired;

        foreach (var (_, person) in _personsOnFloor)
            person.Dispose();

        _personsOnFloor.Clear();
    }
}
