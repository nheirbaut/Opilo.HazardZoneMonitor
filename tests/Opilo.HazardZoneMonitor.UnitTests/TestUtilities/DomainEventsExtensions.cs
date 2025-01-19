using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;

namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

internal static class DomainEventsExtensions
{
    public static async Task<TDomainEvent?> RegisterAndWaitForEvent<TDomainEvent>(TimeSpan? timeout = null)
        where TDomainEvent : IDomainEvent
    {
        var tcs = new TaskCompletionSource<TDomainEvent>();
        DomainEvents.Register<TDomainEvent>(domainEvent => tcs.TrySetResult(domainEvent));

        try
        {
            return await tcs.Task.WaitAsync(timeout ?? TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            return default;
        }
    }
}
