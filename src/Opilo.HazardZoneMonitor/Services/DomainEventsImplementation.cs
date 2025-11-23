using System.Collections.Concurrent;
using Opilo.HazardZoneMonitor.Events;

namespace Opilo.HazardZoneMonitor.Services;

internal sealed class DomainEventsImplementation : IDisposable
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly BlockingCollection<IDomainEvent> _domainEvents = [];
    private readonly TaskCompletionSource<bool> _consumerStarted = new();
    private volatile bool _disposed;

    public void Register<T>(Action<T> domainEventHandler) where T : IDomainEvent
    {
        if (_disposed)
            return;

        var eventType = typeof(T);
        var handlersForType = _handlers.GetOrAdd(eventType, _ => []);
        handlersForType.Add(domainEventHandler);
    }

    public void Raise<T>(T domainEvent) where T : IDomainEvent
    {
        if (_disposed)
            return;

        // Ensure the background thread is actually ready
        _consumerStarted.Task.GetAwaiter().GetResult();

        _domainEvents.Add(domainEvent);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _handlers.Clear();
        ClearDomainEvents();
        _domainEvents.Dispose();
    }

    private void ClearDomainEvents()
    {
        while (_domainEvents.TryTake(out _))
        {
            // Discard
        }
    }
}
