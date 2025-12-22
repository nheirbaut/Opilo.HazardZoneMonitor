using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.Shared.Time;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;

public sealed class Person : IDisposable
{
    private readonly IClock _clock;
    private readonly Shared.Abstractions.ITimer _expiryTimer;
    private readonly IPersonEvents _events;
    private DateTime _lastHeartbeatUtc;

    public Guid Id { get; }
    public Location Location { get; private set; }

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout, IPersonEvents events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var person = new Person(id, location, lifespanTimeout, new SystemClock(), new SystemTimerFactory(), events);
        events.Raise(new PersonCreatedEventArgs(id, location));
        return person;
    }

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout, IClock clock, ITimerFactory timerFactory,
        IPersonEvents events)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(timerFactory);
        ArgumentNullException.ThrowIfNull(events);

        var person = new Person(id, location, lifespanTimeout, clock, timerFactory, events);
        events.Raise(new PersonCreatedEventArgs(id, location));
        return person;
    }

    public void UpdateLocation(Location newLocation)
    {
        _expiryTimer.Stop();
        _expiryTimer.Start();
        _lastHeartbeatUtc = _clock.UtcNow;

        if (newLocation == Location)
            return;

        Location = newLocation;
        _events.Raise(new PersonLocationChangedEvent(Id, Location));
    }

    private Person(Guid id, Location initialLocation, TimeSpan timeout, IClock clock, ITimerFactory timerFactory,
        IPersonEvents events)
    {
        Id = id;
        Location = initialLocation;

        _clock = clock;
        _events = events;
        _expiryTimer = timerFactory.Create(timeout, autoReset: true);
        _expiryTimer.Elapsed += OnExpiryTimerElapsed;
        _expiryTimer.Start();
        _lastHeartbeatUtc = _clock.UtcNow;
    }

    private void OnExpiryTimerElapsed(object? sender, EventArgs e)
    {
        if (_clock.UtcNow < _lastHeartbeatUtc.Add(_expiryTimer.Interval))
            return;

        _events.Raise(new PersonExpiredEvent(Id));
        _expiryTimer.Stop();
    }

    public void Dispose()
    {
        _expiryTimer.Dispose();
    }
}

