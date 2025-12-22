using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class DomainEventsTests : IDisposable
{
    [Fact]
    public async Task Raise_ShouldInvokeHandler_WhenHandlerIsRegistered()
    {
        // Arrange
        var testDomainEvent = new TestDomainEvent();
        var testDomainEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<TestDomainEvent>();

        // Act
        DomainEventDispatcher.Raise(testDomainEvent);
        var receivedDomainEvent = await testDomainEventTask;

        // Assert
        receivedDomainEvent.Should().NotBeNull();
        receivedDomainEvent.Id.Should().Be(testDomainEvent.Id);
    }

    [Fact]
    public void Raise_ShouldNotThrow_WhenNoHandlerIsRegistered()
    {
        // Arrange
        var testDomainEvent = new TestDomainEvent();

        // Act & Assert
        var act = () => DomainEventDispatcher.Raise(testDomainEvent);
        act.Should().NotThrow();
    }

    private sealed class TestDomainEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    public void Dispose()
    {
        DomainEventDispatcher.Dispose();
    }
}
