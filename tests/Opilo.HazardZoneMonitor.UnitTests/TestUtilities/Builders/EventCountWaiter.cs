using System.Collections.Concurrent;
using Opilo.HazardZoneMonitor.Events.HazardZoneEvents;

namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities.Builders;

internal sealed class EventCountWaiter(int expectedCount)
{
    private int _currentCount;
    private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // ReSharper disable once CollectionNeverQueried.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public ConcurrentBag<PersonAddedToHazardZoneEvent> ReceivedEvents { get;  } = [];

    public void Signal(PersonAddedToHazardZoneEvent personAddedToHazardZoneEvent)
    {
        ReceivedEvents.Add(personAddedToHazardZoneEvent);
        if (Interlocked.Increment(ref _currentCount) == expectedCount)
            _tcs.TrySetResult(true);
    }

    public List<PersonAddedToHazardZoneEvent> Wait(TimeSpan timeout)
    {
        if (!_tcs.Task.Wait(timeout))
            throw new TimeoutException(
                $"Timed out waiting for {expectedCount} events. Only {_currentCount} events were received.");

        return ReceivedEvents.ToList();
    }
}
