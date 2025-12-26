namespace Opilo.HazardZoneMonitor.Tests.Unit.TestUtilities;

internal sealed class FakeTimer : Shared.Abstractions.ITimer
{
    private readonly FakeClock _clock;
    private readonly Lock _lock = new();

    private DateTime? _nextDueAtUtc;
    private volatile bool _disposed;

    public TimeSpan Interval { get; set; }
    public bool AutoReset { get; set; }
    public bool Enabled { get; private set; }

    public event EventHandler? Elapsed;

    public FakeTimer(FakeClock clock, TimeSpan interval, bool autoReset)
    {
        _clock = clock;
        Interval = interval;
        AutoReset = autoReset;
        _clock.Register(this);
    }

    public void Start()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            Enabled = true;
            if (Interval <= TimeSpan.Zero)
            {
                // Fire immediately at the current time.
                Fire_NoLock(_clock.UtcNow);
                return;
            }

            _nextDueAtUtc = _clock.UtcNow.Add(Interval);
        }
    }

    public void Stop()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            Enabled = false;
            _nextDueAtUtc = null;
        }
    }

    internal bool TryGetNextDueAt(out DateTime dueAtUtc)
    {
        lock (_lock)
        {
            if (!Enabled || _nextDueAtUtc is null)
            {
                dueAtUtc = default;
                return false;
            }

            dueAtUtc = _nextDueAtUtc.Value;
            return true;
        }
    }

    internal void Fire_NoLock(DateTime nowUtc)
    {
        if (_disposed)
            return;

        EventHandler? handler;

        lock (_lock)
        {
            if (_disposed)
                return;

            if (!Enabled)
                return;

            handler = Elapsed;

            if (AutoReset && Interval > TimeSpan.Zero)
            {
                // Schedule next occurrence relative to now.
                _nextDueAtUtc = nowUtc.Add(Interval);
            }
            else
            {
                Enabled = false;
                _nextDueAtUtc = null;
            }
        }

        if (_disposed)
            return;

        handler?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _clock.Unregister(this);

        lock (_lock)
        {
            Enabled = false;
            _nextDueAtUtc = null;
            Elapsed = null;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
