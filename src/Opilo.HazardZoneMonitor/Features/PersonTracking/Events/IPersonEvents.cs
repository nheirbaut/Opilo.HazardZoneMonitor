using Opilo.HazardZoneMonitor.Shared.Events;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public interface IPersonEvents
{
    event EventHandler<DomainEventArgs<PersonCreatedEvent>>? Created;
    event EventHandler<DomainEventArgs<PersonLocationChangedEvent>>? LocationChanged;
    event EventHandler<DomainEventArgs<PersonExpiredEvent>>? Expired;

    void Raise(PersonCreatedEvent personCreatedEvent);
    void Raise(PersonLocationChangedEvent personLocationChangedEvent);
    void Raise(PersonExpiredEvent personExpiredEvent);
}
