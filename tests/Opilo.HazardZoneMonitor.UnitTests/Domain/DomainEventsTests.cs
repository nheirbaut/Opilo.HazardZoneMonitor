using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class DomainEventsTests : IDisposable
{
    [Fact]
    public async Task Raise_WhenHandlerIsRegistered_InvokesHandler()
    {
        // Arrange
        var testDomainEvent = new TestDomainEvent();
        var testDomainEventTask = DomainEventsExtensions.Register<TestDomainEvent>();

        // Act
        DomainEvents.Raise(testDomainEvent);
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
        var ex = Record.Exception(() => DomainEvents.Raise(testDomainEvent));
        Assert.Null(ex);
    }

    private sealed class TestDomainEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    public void Dispose()
    {
        DomainEvents.Reset();
    }
}
