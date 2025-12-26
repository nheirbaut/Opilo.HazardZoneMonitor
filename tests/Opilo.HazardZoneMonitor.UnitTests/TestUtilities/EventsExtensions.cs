namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

internal static class EventsExtensions
{
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
            return null;
        }
    }
}
