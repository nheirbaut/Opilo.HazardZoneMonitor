using System.Diagnostics.CodeAnalysis;
using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Test utility; instantiated by tests.")]
internal sealed class FakeClock : IClock
{
    private readonly Lock _lock = new();
    private readonly HashSet<FakeTimer> _timers = [];

    public DateTime UtcNow { get; private set; }

    public FakeClock(DateTime? initialUtcNow = null)
    {
        UtcNow = initialUtcNow ?? DateTime.UnixEpoch;
    }

    public void AdvanceBy(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delta), delta, "Cannot advance backwards.");

        lock (_lock)
        {
            UtcNow = UtcNow.Add(delta);
            FireDueTimers_NoLock();
        }
    }

    internal void Register(FakeTimer timer)
    {
        lock (_lock)
        {
            _timers.Add(timer);
        }
    }

    internal void Unregister(FakeTimer timer)
    {
        lock (_lock)
        {
            _timers.Remove(timer);
        }
    }

    private void FireDueTimers_NoLock()
    {
        // Deterministic: always fire the next soonest timer first.
        while (true)
        {
            FakeTimer? nextTimer = null;
            DateTime nextDueAt = DateTime.MaxValue;

            foreach (var timer in _timers)
            {
                if (!timer.TryGetNextDueAt(out var dueAt))
                    continue;

                if (dueAt <= UtcNow && dueAt <= nextDueAt)
                {
                    nextDueAt = dueAt;
                    nextTimer = timer;
                }
            }

            if (nextTimer is null)
                return;

            nextTimer.Fire_NoLock(UtcNow);
        }
    }
}
