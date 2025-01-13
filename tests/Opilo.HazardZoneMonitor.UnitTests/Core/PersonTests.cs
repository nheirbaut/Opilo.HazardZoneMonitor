using Opilo.HazardZoneMonitor.Core;
using Opilo.HazardZoneMonitor.Core.Services;

namespace Opilo.HazardZoneMonitor.UnitTests.Core;

public sealed class PersonTests : IDisposable
{
    private Person? _person;

    [Fact]
    public void Create_WhenCreatingPerson_ShouldRaisePersonCreatedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);
        PersonCreatedEvent? personCreatedEvent = null;

        DomainEvents.Register<PersonCreatedEvent>(e => personCreatedEvent = e);

        // Act
        _person = Person.Create(personId, location, timeout);

        // Assert
        Assert.NotNull(personCreatedEvent);
        Assert.Equal(personId, personCreatedEvent.Person.Id);
        Assert.Equal(location, personCreatedEvent.Person.Location);
    }

    [Fact]
    public void UpdateLocation_WhenUpdatingLocation_ShouldRaisePersonLocationChangedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var initialLocation = new Location(0, 0);
        var newLocation = new Location(1, 1);
        var timeout = TimeSpan.FromSeconds(1);
        PersonLocationChangedEvent? personLocationChangedEvent = null;

        DomainEvents.Register<PersonLocationChangedEvent>(e => personLocationChangedEvent = e);
        _person = Person.Create(personId, initialLocation, timeout);

        // Act
        _person.UpdateLocation(newLocation);

        // Assert
        Assert.NotNull(personLocationChangedEvent);
        Assert.Equal(personId, personLocationChangedEvent.Person.Id);
        Assert.Equal(newLocation, personLocationChangedEvent.Person.Location);
    }

    [Fact]
    public async Task ExpirePerson_WhenTimeExpires_ShouldRaisePersonExpiredEvent()
    {
        // Arrange
        var lifespanTimeout = TimeSpan.FromMilliseconds(10);
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        PersonExpiredEvent? personExpiredEvent = null;

        DomainEvents.Register<PersonExpiredEvent>(e => personExpiredEvent = e);
        _person = Person.Create(personId, location, lifespanTimeout);

        // Act
        await Task.Delay(lifespanTimeout * 2);

        // Assert
        Assert.NotNull(personExpiredEvent);
        Assert.Equal(personId, personExpiredEvent.Person.Id);
    }

    public void Dispose()
    {
        _person?.Dispose();
    }
}
