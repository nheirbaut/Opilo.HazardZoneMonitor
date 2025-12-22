using Opilo.HazardZoneMonitor.Shared.Abstractions;

using TimersTimer = System.Timers.Timer;

namespace Opilo.HazardZoneMonitor.Shared.Time;

public sealed class SystemTimer : Opilo.HazardZoneMonitor.Shared.Abstractions.ITimer
{
    private readonly TimersTimer _timer;

    public TimeSpan Interval
    {
        get => TimeSpan.FromMilliseconds(_timer.Interval);
        set => _timer.Interval = value.TotalMilliseconds;
    }

    public bool AutoReset
    {
        get => _timer.AutoReset;
        set => _timer.AutoReset = value;
    }

    public bool Enabled => _timer.Enabled;

    public event EventHandler? Elapsed;

    public SystemTimer(TimeSpan interval, bool autoReset)
    {
        _timer = new TimersTimer(interval) { AutoReset = autoReset };
        _timer.Elapsed += OnElapsed;
    }

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    public void Dispose()
    {
        _timer.Stop();
        _timer.Elapsed -= OnElapsed;
        _timer.Dispose();
    }

    private void OnElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Elapsed?.Invoke(this, EventArgs.Empty);
    }
}
