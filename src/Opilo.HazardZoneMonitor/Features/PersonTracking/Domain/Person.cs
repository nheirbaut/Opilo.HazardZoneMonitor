using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;

public sealed class Person : IDisposable
{
    private readonly Shared.Abstractions.ITimer _expiryTimer;

    public Guid Id { get; }
    public Location Location { get; private set; }

    public event EventHandler<PersonLocationChangedEventArgs>? LocationChanged;
    public event EventHandler<PersonExpiredEventArgs>? Expired;

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout, IClock clock, ITimerFactory timerFactory)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(timerFactory);

        return new Person(id, location, lifespanTimeout, clock, timerFactory);
    }

    public void UpdateLocation(Location newLocation)
    {
        // Any update counts as a heartbeat; extend the expiration.
        RescheduleExpiry();

        if (newLocation == Location)
            return;

        Location = newLocation;
        LocationChanged?.Invoke(this, new PersonLocationChangedEventArgs(Id, Location));
    }

    private Person(Guid id, Location initialLocation, TimeSpan timeout, IClock clock, ITimerFactory timerFactory)
    {
        Id = id;
        Location = initialLocation;

        // One-shot timer: we restart it on each heartbeat.
        _expiryTimer = timerFactory.Create(timeout, autoReset: false);
        _expiryTimer.Elapsed += OnExpiryTimerElapsed;
        _expiryTimer.Start();
    }

    private void RescheduleExpiry()
    {
        // Restart the one-shot timer so it expires timeout after the last heartbeat.
        _expiryTimer.Stop();
        _expiryTimer.Start();
    }

    private void OnExpiryTimerElapsed(object? sender, EventArgs e)
    {
        Expired?.Invoke(this, new PersonExpiredEventArgs(Id));
    }

    public void Dispose() => _expiryTimer.Dispose();
}

