using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class PersonTests : IDisposable
{
    private Person? _person;

    [Fact]
    public void Create_GivenValidParameters_CreatesValidPerson()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);

        // Act
        _person = Person.Create(personId, location, timeout);

        // Assert
        Assert.NotNull(_person);
        Assert.Equal(personId, _person.Id);
        Assert.Equal(location, _person.Location);
    }

    [Fact]
    public void Create_GivenValidParameters_RaisesPersonCreatedEvent()
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
    public void UpdateLocation_WithNewLocation_RaisesPersonLocationChangedEvent()
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
    public async Task ExpirePerson_WhenTimeExpires_RaisesPersonExpiredEvent()
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
