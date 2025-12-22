using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Shared.Time;

public sealed class SystemTimerFactory : ITimerFactory
{
    public Opilo.HazardZoneMonitor.Shared.Abstractions.ITimer Create(TimeSpan interval, bool autoReset = false)
        => new SystemTimer(interval, autoReset);
}
