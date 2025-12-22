using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.Shared.Time;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;

public sealed class Person : IDisposable
{
    private readonly IClock _clock;
    private readonly Opilo.HazardZoneMonitor.Shared.Abstractions.ITimer _expiryTimer;
    private DateTime _lastHeartbeatUtc;

    public Guid Id { get; }
    public Location Location { get; private set; }

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout)
    {
        var person = new Person(id, location, lifespanTimeout, new SystemClock(), new SystemTimerFactory());
        DomainEventDispatcher.Raise(new PersonCreatedEvent(id, location));
        return person;
    }

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout, IClock clock, ITimerFactory timerFactory)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(timerFactory);

        var person = new Person(id, location, lifespanTimeout, clock, timerFactory);
        DomainEventDispatcher.Raise(new PersonCreatedEvent(id, location));
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
        DomainEventDispatcher.Raise(new PersonLocationChangedEvent(Id, Location));
    }

    private Person(Guid id, Location initialLocation, TimeSpan timeout, IClock clock, ITimerFactory timerFactory)
    {
        Id = id;
        Location = initialLocation;

        _clock = clock;
        _expiryTimer = timerFactory.Create(timeout, autoReset: true);
        _expiryTimer.Elapsed += OnExpiryTimerElapsed;
        _expiryTimer.Start();
        _lastHeartbeatUtc = _clock.UtcNow;
    }

    private void OnExpiryTimerElapsed(object? sender, EventArgs e)
    {
        if (_clock.UtcNow < _lastHeartbeatUtc.Add(_expiryTimer.Interval))
            return;

        DomainEventDispatcher.Raise(new PersonExpiredEvent(Id));
        _expiryTimer.Stop();
    }

    public void Dispose()
    {
        _expiryTimer.Dispose();
    }
}

