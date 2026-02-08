namespace Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;

public interface ITimer : IDisposable
{
    TimeSpan Interval { get; set; }
    bool AutoReset { get; set; }
    bool Enabled { get; }

    event EventHandler? Elapsed;

    void Start();
    void Stop();
}
