using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Shared.Time;

public sealed class SystemTimerFactory : ITimerFactory
{
    public Abstractions.ITimer Create(TimeSpan interval, bool autoReset = false)
        => new SystemTimer(interval, autoReset);
}
