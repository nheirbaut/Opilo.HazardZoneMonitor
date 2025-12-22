using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class DomainEventsTests : IDisposable
{
    [Fact]
    public async Task Raise_WhenHandlerIsRegistered_InvokesHandler()
    {
        // Arrange
        var testDomainEvent = new TestDomainEvent();
        var testDomainEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<TestDomainEvent>();

        // Act
        DomainEventDispatcher.Raise(testDomainEvent);
        var receivedDomainEvent = await testDomainEventTask;

        // Assert
        Assert.NotNull(receivedDomainEvent);
        Assert.Equal(testDomainEvent.Id, receivedDomainEvent.Id);
    }

    [Fact]
    public void Raise_WhenNoHandlerRegistered_DoesNotThrow()
    {
        // Arrange
        var testDomainEvent = new TestDomainEvent();

        // Act & Assert
        var ex = Record.Exception(() => DomainEventDispatcher.Raise(testDomainEvent));
        Assert.Null(ex);
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
