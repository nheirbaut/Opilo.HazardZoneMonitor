using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.PersonTracking.Domain;

public sealed class Person : IDisposable
{
    private readonly Shared.Abstractions.ITimer _expiryTimer;

    public Guid Id { get; }
    public Location Location { get; private set; }

    public event EventHandler<PersonLocationChangedEventArgs>? LocationChanged;
    public event EventHandler<PersonExpiredEventArgs>? Expired;

    public static Person Create(Guid id, Location location, TimeSpan lifespanTimeout, ITimerFactory timerFactory)
    {
        ArgumentNullException.ThrowIfNull(timerFactory);

        return new Person(id, location, lifespanTimeout, timerFactory);
    }

    public bool TryLocationUpdate(PersonLocationUpdate personLocationUpdate)
    {
        Guard.Against.Null(personLocationUpdate);

        if (personLocationUpdate.PersonId != Id)
            return false;

        RescheduleExpiry();

        if (personLocationUpdate.Location == Location)
            return true;

        Location = personLocationUpdate.Location;
        LocationChanged?.Invoke(this, new PersonLocationChangedEventArgs(Id, Location));

        return true;
    }

    private Person(Guid id, Location initialLocation, TimeSpan timeout, ITimerFactory timerFactory)
    {
        Id = id;
        Location = initialLocation;

        _expiryTimer = timerFactory.Create(timeout, autoReset: false);
        _expiryTimer.Elapsed += OnExpiryTimerElapsed;
        _expiryTimer.Start();
    }

    private void RescheduleExpiry()
    {
        _expiryTimer.Stop();
        _expiryTimer.Start();
    }

    private void OnExpiryTimerElapsed(object? sender, EventArgs e)
    {
        Expired?.Invoke(this, new PersonExpiredEventArgs(Id));
    }

    public void Dispose() => _expiryTimer.Dispose();
}

