using Opilo.HazardZoneMonitor.Shared.Events;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public sealed class PersonEvents : IPersonEvents
{
    public event EventHandler<PersonCreatedEventArgs>? Created;
    public event EventHandler<DomainEventArgs<PersonLocationChangedEvent>>? LocationChanged;
    public event EventHandler<DomainEventArgs<PersonExpiredEvent>>? Expired;

    public void Raise(PersonCreatedEventArgs personCreatedEvent)
    {
        ArgumentNullException.ThrowIfNull(personCreatedEvent);
        Created?.Invoke(this, new PersonCreatedEventArgs(personCreatedEvent.PersonId, personCreatedEvent.Location));
    }

    public void Raise(PersonLocationChangedEvent personLocationChangedEvent)
    {
        ArgumentNullException.ThrowIfNull(personLocationChangedEvent);
        LocationChanged?.Invoke(this, new DomainEventArgs<PersonLocationChangedEvent>(personLocationChangedEvent));
    }

    public void Raise(PersonExpiredEvent personExpiredEvent)
    {
        ArgumentNullException.ThrowIfNull(personExpiredEvent);
        Expired?.Invoke(this, new DomainEventArgs<PersonExpiredEvent>(personExpiredEvent));
    }
}
