using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.Shared.Time;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;

public sealed class Person : IDisposable
{
    public static event EventHandler<DomainEventArgs<PersonCreatedEvent>>? Created;
    public static event EventHandler<DomainEventArgs<PersonLocationChangedEvent>>? LocationChanged;
    public static event EventHandler<DomainEventArgs<PersonExpiredEvent>>? Expired;

    private readonly IClock _clock;
    private readonly Opilo.HazardZoneMonitor.Shared.Abstractions.ITimer _expiryTimer;
    private DateTime _lastHeartbeatUtc;

    public Guid Id { get; }
    public Location Location { get; private set; }

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout)
    {
        var person = new Person(id, location, lifespanTimeout, new SystemClock(), new SystemTimerFactory());
        OnCreated(new PersonCreatedEvent(id, location));
        return person;
    }

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout, IClock clock, ITimerFactory timerFactory)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(timerFactory);

        var person = new Person(id, location, lifespanTimeout, clock, timerFactory);
        OnCreated(new PersonCreatedEvent(id, location));
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
        OnLocationChanged(new PersonLocationChangedEvent(Id, Location));
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

        OnExpired(new PersonExpiredEvent(Id));
        _expiryTimer.Stop();
    }

    public void Dispose()
    {
        _expiryTimer.Dispose();
    }

    private static void OnCreated(PersonCreatedEvent e)
    {
        var handlers = Created;
        handlers?.Invoke(null, new DomainEventArgs<PersonCreatedEvent>(e));
    }

    private static void OnLocationChanged(PersonLocationChangedEvent e)
    {
        var handlers = LocationChanged;
        handlers?.Invoke(null, new DomainEventArgs<PersonLocationChangedEvent>(e));
    }

    private static void OnExpired(PersonExpiredEvent e)
    {
        var handlers = Expired;
        handlers?.Invoke(null, new DomainEventArgs<PersonExpiredEvent>(e));
    }
}

