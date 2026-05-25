using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using DomainTimer = Opilo.HazardZoneMonitor.Domain.Shared.Abstractions.ITimer;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;

internal sealed class FakeTimerFactory(FakeClock clock) : ITimerFactory
{
    public DomainTimer Create(TimeSpan interval, bool autoReset = false)
        => new FakeTimer(clock, interval, autoReset);
}
