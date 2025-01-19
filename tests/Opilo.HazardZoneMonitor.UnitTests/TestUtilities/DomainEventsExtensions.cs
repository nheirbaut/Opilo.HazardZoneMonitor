using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;

namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

internal static class DomainEventsExtensions
{
    public static async Task<TDomainEvent?> Register<TDomainEvent>(TimeSpan? timeout = null) where TDomainEvent : IDomainEvent
    {
        var tcs = new TaskCompletionSource<TDomainEvent>();

        DomainEvents.Register<TDomainEvent>(domainEvent => tcs.TrySetResult(domainEvent));

        using var cancellationTokenSource = new CancellationTokenSource(timeout ?? TimeSpan.FromMilliseconds(500));

        try
        {
            await using (cancellationTokenSource.Token.Register(() => tcs.TrySetCanceled(),
                             useSynchronizationContext: false))
            {
                return await tcs.Task.ConfigureAwait(false);
            }
        }
        catch (TaskCanceledException)
        {
            // Timeout occurred
            return default;
        }
    }
}
