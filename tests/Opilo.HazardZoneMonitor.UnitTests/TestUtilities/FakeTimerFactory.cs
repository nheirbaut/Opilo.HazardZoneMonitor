using System.Diagnostics.CodeAnalysis;
using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Test utility; instantiated by tests.")]
internal sealed class FakeTimerFactory : ITimerFactory
{
    private readonly FakeClock _clock;

    public FakeTimerFactory(FakeClock clock)
    {
        _clock = clock;
    }

    public Shared.Abstractions.ITimer Create(TimeSpan interval, bool autoReset = false)
        => new FakeTimer(_clock, interval, autoReset);
}
