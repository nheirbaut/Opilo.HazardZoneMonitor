﻿using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Events;

namespace Opilo.HazardZoneMonitor.Services;

#pragma warning disable S1133 // Deprecated code should be removed eventually
[Obsolete("Use DomainEventDispatcher from Opilo.HazardZoneMonitor.Shared.Events instead")]
public static class DomainEvents
{
    public static void Register<T>(Action<T> handler) where T : IDomainEvent
    {
        DomainEventDispatcher.Register(handler);
    }

    public static void Raise<T>(T domainEvent) where T : IDomainEvent
    {
        DomainEventDispatcher.Raise(domainEvent);
    }

    public static void Dispose()
    {
        DomainEventDispatcher.Dispose();
    }
}
#pragma warning restore S1133
