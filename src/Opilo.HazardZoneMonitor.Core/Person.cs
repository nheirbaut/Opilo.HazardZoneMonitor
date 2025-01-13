namespace Opilo.HazardZoneMonitor.Core;

public sealed class Person
{
    private readonly TimeSpan _time;

    public Guid Id { get; init; }
    public Location Location { get; private set; }

    public static Person Create(Guid id, Location location, TimeSpan timeout)
    {
        var person = new Person(id, location, timeout);
        DomainEvents.Raise(new PersonCreatedEvent(person));
        return person;
    }

    public void UpdateLocation(Location newLocation)
    {
        var previousLocation = Location;
        Location = newLocation;
        DomainEvents.Raise(new PersonLocationChangedEvent(this, previousLocation, newLocation));
    }

    private Person(Guid id, Location initialLocation, TimeSpan timeout)
    {
        Id = id;
        Location = initialLocation;
        _time = timeout;
    }
}
