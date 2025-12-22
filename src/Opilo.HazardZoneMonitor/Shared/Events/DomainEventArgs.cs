using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Shared.Events;

public sealed class DomainEventArgs<TDomainEvent> : EventArgs
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventArgs(TDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        DomainEvent = domainEvent;
    }
}
