namespace Opilo.HazardZoneMonitor.Shared.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
