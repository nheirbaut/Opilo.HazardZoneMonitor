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

    [Fact]
    public void UpdateLocation_WhenUpdatingLocation_ShouldRaisePersonLocationChangedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var initialLocation = new Location(0, 0);
        var newLocation = new Location(1, 1);
        var timeout = TimeSpan.FromSeconds(1);
        bool eventRaised = false;

        DomainEvents.Register<PersonLocationChangedEvent>(_ => eventRaised = true);
        var person = Person.Create(personId, initialLocation, timeout);

        // Act
        person.UpdateLocation(newLocation);

        // Assert
        Assert.True(eventRaised);
    }
}
