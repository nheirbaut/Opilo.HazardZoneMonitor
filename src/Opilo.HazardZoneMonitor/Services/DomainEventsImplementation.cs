﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Events;

namespace Opilo.HazardZoneMonitor.Services;

#pragma warning disable S1133 // Deprecated code should be removed eventually
#pragma warning disable S2325 // Make methods static - Cannot be static as this is a wrapper for backward compatibility
#pragma warning disable CA1812 // Internal class is instantiated via reflection
[Obsolete("Use DomainEventDispatcherImplementation from Opilo.HazardZoneMonitor.Shared.Events instead")]
internal sealed class DomainEventsImplementation : IDisposable
{
    private readonly DomainEventDispatcherImplementation _implementation = new();

    public void Register<T>(Action<T> domainEventHandler) where T : IDomainEvent
    {
        DomainEventDispatcher.Register(domainEventHandler);
    }

    public void Raise<T>(T domainEvent) where T : IDomainEvent
    {
        DomainEventDispatcher.Raise(domainEvent);
    }

    public void Dispose()
    {
        _implementation.Dispose();
    }
}
#pragma warning restore CA1812
#pragma warning restore S2325
#pragma warning restore S1133
