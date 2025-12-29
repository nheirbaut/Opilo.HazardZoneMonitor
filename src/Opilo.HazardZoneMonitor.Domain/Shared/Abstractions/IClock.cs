namespace Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
