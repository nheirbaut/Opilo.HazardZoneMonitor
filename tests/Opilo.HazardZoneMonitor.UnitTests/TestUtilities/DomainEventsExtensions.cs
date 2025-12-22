namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

using Shared.Events;
using Shared.Abstractions;

internal static class DomainEventsExtensions
{
    public static async Task<TDomainEvent?> RegisterAndWaitForEvent<TDomainEvent>(
        Action<EventHandler<DomainEventArgs<TDomainEvent>>> subscribe,
        Action<EventHandler<DomainEventArgs<TDomainEvent>>> unsubscribe,
        TimeSpan? timeout = null)
        where TDomainEvent : class, IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);

        var tcs = new TaskCompletionSource<TDomainEvent?>(TaskCreationOptions.RunContinuationsAsynchronously);

        EventHandler<DomainEventArgs<TDomainEvent>> handler = null!;
        handler = (_, args) =>
        {
            unsubscribe(handler);
            tcs.TrySetResult(args.DomainEvent);
        };

        subscribe(handler);

        try
        {
            return await tcs.Task.WaitAsync(timeout ?? TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            unsubscribe(handler);
            return default;
        }
    }

    public static async Task<TEventArgs?> RegisterAndWaitForEvent<TEventArgs>(
        Action<EventHandler<TEventArgs>> subscribe,
        Action<EventHandler<TEventArgs>> unsubscribe,
        TimeSpan? timeout = null)
        where TEventArgs : EventArgs
    {
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);

        var tcs = new TaskCompletionSource<TEventArgs?>(TaskCreationOptions.RunContinuationsAsynchronously);

        EventHandler<TEventArgs> handler = null!;
        handler = (_, args) =>
        {
            unsubscribe(handler);
            tcs.TrySetResult(args);
        };

        subscribe(handler);

        try
        {
            return await tcs.Task.WaitAsync(timeout ?? TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            unsubscribe(handler);
            return default;
        }
    }
}
