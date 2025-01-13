using Opilo.HazardZoneMonitor.Core;

namespace Opilo.HazardZoneMonitor.UnitTests.Core;

public class PersonTests
{
    [Fact]
    public void Create_WhenCreatingPerson_ShouldRaisePersonCreatedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);
        bool eventRaised = false;

        DomainEvents.Register<PersonCreatedEvent>(_ => eventRaised = true);

        // Act
        Person.Create(personId, location, timeout);

        // Assert
        Assert.True(eventRaised);
    }
}
