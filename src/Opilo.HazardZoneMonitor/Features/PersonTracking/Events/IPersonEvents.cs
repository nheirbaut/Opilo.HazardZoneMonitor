using Opilo.HazardZoneMonitor.Shared.Events;

namespace Opilo.HazardZoneMonitor.Features.PersonTracking.Events;

public interface IPersonEvents
{
    event EventHandler<PersonCreatedEventArgs>? Created;
    event EventHandler<DomainEventArgs<PersonLocationChangedEvent>>? LocationChanged;
    event EventHandler<DomainEventArgs<PersonExpiredEvent>>? Expired;

    void Raise(PersonCreatedEventArgs personCreatedEvent);
    void Raise(PersonLocationChangedEvent personLocationChangedEvent);
    void Raise(PersonExpiredEvent personExpiredEvent);
}
