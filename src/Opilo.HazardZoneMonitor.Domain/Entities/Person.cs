using System.Timers;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Timer = System.Timers.Timer;

namespace Opilo.HazardZoneMonitor.Domain.Entities;

public sealed class Person : IDisposable
{
    private DateTime _initialTime;
    private readonly Timer _expiryTimer;

    public Guid Id { get; }
    public Location Location { get; private set; }

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout)
    {
        var person = new Person(id, location, lifespanTimeout);
        DomainEvents.Raise(new PersonCreatedEvent(person));
        return person;
    }

    public void UpdateLocation(Location newLocation)
    {
        _expiryTimer.Stop();
        _expiryTimer.Start();
        _initialTime = DateTime.UtcNow;

        if (newLocation == Location)
            return;

        var previousLocation = Location;

        Location = newLocation;
        DomainEvents.Raise(new PersonLocationChangedEvent(Id, Location, previousLocation));
    }

    private Person(Guid id, Location initialLocation, TimeSpan timeout)
    {
        Id = id;
        Location = initialLocation;
        _expiryTimer = new Timer(timeout.TotalMilliseconds) { AutoReset = true };
        _expiryTimer.Elapsed += OnExpiryTimerElapsed;
        _expiryTimer.Start();
        _initialTime = DateTime.UtcNow;
    }

    private void OnExpiryTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (DateTime.UtcNow < _initialTime.AddMilliseconds(_expiryTimer.Interval))
            return;

        DomainEvents.Raise(new PersonExpiredEvent(Id));
        _expiryTimer.Stop();
    }

    public void Dispose()
    {
        _expiryTimer.Dispose();
    }
}
