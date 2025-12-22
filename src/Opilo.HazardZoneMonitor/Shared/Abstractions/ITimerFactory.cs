namespace Opilo.HazardZoneMonitor.Shared.Abstractions;

public interface ITimerFactory
{
    ITimer Create(TimeSpan interval, bool autoReset = false);
}
