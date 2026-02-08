using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
