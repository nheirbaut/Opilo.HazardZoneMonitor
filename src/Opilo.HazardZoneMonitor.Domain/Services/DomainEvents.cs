using System.Collections.Concurrent;
using Opilo.HazardZoneMonitor.Domain.Events;

namespace Opilo.HazardZoneMonitor.Domain.Services;

public static class DomainEvents
{
    private static Lazy<DomainEventsImplementation> s_implementation = new(
        () => new DomainEventsImplementation(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static void Register<T>(Action<T> domainEventHandler) where T : IDomainEvent
    {
        s_implementation.Value.Register(domainEventHandler);
    }

    public static void Raise<T>(T domainEvent) where T : IDomainEvent
    {
        s_implementation.Value.Raise(domainEvent);
    }

    public static void Dispose()
    {
        if (s_implementation is { IsValueCreated: true, Value.IsDisposed: false })
            s_implementation.Value.Dispose();
    }

    public static void Reset()
    {
        Dispose();

        s_implementation = new Lazy<DomainEventsImplementation>(
            () => new DomainEventsImplementation(),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }
}

internal sealed class DomainEventsImplementation : IDisposable
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = [];
    private readonly BlockingCollection<IDomainEvent> _domainEvents = [];
    private volatile bool _disposed;

    private readonly TaskCompletionSource<bool> _consumerStarted = new();

    public bool IsDisposed => _disposed;

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
        var listForType = _handlers.GetOrAdd(eventType, _ => []);

        listForType.Add(domainEventHandler);
    }

    public void Raise<T>(T domainEvent) where T : IDomainEvent
    {
        if (_disposed)
            return;

        _consumerStarted.Task.GetAwaiter().GetResult();

        _domainEvents.Add(domainEvent);
    }

    private void StartConsumingLoopAsync()
    {
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
        if (_disposed)
            return;

        _disposed = true;

        _handlers.Clear();
        _domainEvents.Dispose();
    }
}
