namespace Opilo.HazardZoneMonitor.Core;

public sealed class Person
{
    private readonly Guid _id;
    private readonly Location _location;
    private readonly TimeSpan _time;

    public static Person Create(Guid id, Location location, TimeSpan timeout)
    {
        var person = new Person(id, location, timeout);
        DomainEvents.Raise(new PersonCreatedEvent(person));
        return person;
    }

    private Person(Guid id, Location location, TimeSpan timeout)
    {
        _id = id;
        _location = location;
        _time = timeout;
    }
}
