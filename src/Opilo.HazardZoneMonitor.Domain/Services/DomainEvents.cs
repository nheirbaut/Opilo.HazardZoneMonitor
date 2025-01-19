using System.Collections.Concurrent;
using Opilo.HazardZoneMonitor.Domain.Events;

namespace Opilo.HazardZoneMonitor.Domain.Services;

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

internal sealed class DomainEventsImplementation : IDisposable
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly BlockingCollection<IDomainEvent> _domainEvents = [];
    private readonly TaskCompletionSource<bool> _consumerStarted = new();
    private volatile bool _disposed;

    public DomainEventsImplementation()
    {
        Task.Factory.StartNew(
            StartConsumingLoopAsync,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

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

        // Ensure background thread is actually ready
        _consumerStarted.Task.GetAwaiter().GetResult();

        _domainEvents.Add(domainEvent);
    }

    private void StartConsumingLoopAsync()
    {
        // Let any waiting calls know we are ready
        _consumerStarted.SetResult(true);

        while (!_domainEvents.IsAddingCompleted)
        {
            var domainEvent = _domainEvents.Take();
            HandleDomainEvent(domainEvent);
        }
    }

    private void HandleDomainEvent(IDomainEvent domainEvent)
    {
        if (!_handlers.TryGetValue(domainEvent.GetType(), out var handlersForType))
            return;

        foreach (var handler in handlersForType)
            handler.DynamicInvoke(domainEvent);
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
