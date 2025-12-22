using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Shared.Events;

public static class DomainEventDispatcher
{
    private static DomainEventDispatcherImplementation? s_instance = new();

    public static void Register<T>(Action<T> handler) where T : IDomainEvent
    {
        s_instance?.Register(handler);
    }

    public static void Raise<T>(T domainEvent) where T : IDomainEvent
    {
        s_instance?.Raise(domainEvent);
    }

    public static void Dispose()
    {
        s_instance?.Dispose();
        s_instance = new DomainEventDispatcherImplementation();
    }
}

