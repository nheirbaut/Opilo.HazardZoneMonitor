using System.Collections.Concurrent;
using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Shared.Events;

internal sealed class DomainEventDispatcherImplementation : IDisposable
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private volatile bool _disposed;

    public void Register<T>(Action<T> domainEventHandler) where T : IDomainEvent
    {
        if (_disposed)
            return;

        var eventType = typeof(T);
        var handlersForType = _handlers.GetOrAdd(eventType, _ => []);
        lock (handlersForType)
        {
            handlersForType.Add(domainEventHandler);
        }
    }

    public void Raise<T>(T domainEvent) where T : IDomainEvent
    {
        if (_disposed)
            return;

        var eventType = typeof(T);
        if (!_handlers.TryGetValue(eventType, out var handlersForType))
            return;

        List<Delegate> handlersCopy;
        lock (handlersForType)
        {
            handlersCopy = [.. handlersForType];
        }

        foreach (var handler in handlersCopy)
        {
            if (handler is Action<T> typedHandler)
            {
                typedHandler(domainEvent);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _handlers.Clear();
    }
}

