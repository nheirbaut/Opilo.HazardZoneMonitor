using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Time;

public sealed class SystemTimerFactory : ITimerFactory
{
    public Abstractions.ITimer Create(TimeSpan interval, bool autoReset = false)
        => new SystemTimer(interval, autoReset);
}
