using Opilo.HazardZoneMonitor.Events;

namespace Opilo.HazardZoneMonitor.Services;

public static class DomainEvents
{
    private static DomainEventsImplementation? s_instance = new();

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
        s_instance = new DomainEventsImplementation();
    }
}
