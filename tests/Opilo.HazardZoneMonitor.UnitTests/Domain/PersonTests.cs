using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Events;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class PersonTests : IDisposable
{
    Person? _testPerson;

    [Fact]
    public void Create_GivenValidParameters_CreatesValidPerson()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);

        // Act
        _testPerson = Person.Create(personId, location, timeout);

        // Assert
        Assert.NotNull(_testPerson);
        Assert.Equal(personId, _testPerson.Id);
        Assert.Equal(location, _testPerson.Location);
    }

    [Fact]
    public async Task Create_GivenValidParameters_RaisesPersonCreatedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);
        var personCreatedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonCreatedEvent>();

        // Act
        _testPerson = Person.Create(personId, location, timeout);
        var personCreatedEvent = await personCreatedEventTask;

        // Assert
        Assert.NotNull(personCreatedEvent);
        Assert.Equal(personId, personCreatedEvent.Person.Id);
        Assert.Equal(location, personCreatedEvent.Person.Location);
    }

    [Fact]
    public async Task UpdateLocation_WithNewLocation_RaisesPersonLocationChangedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var initialLocation = new Location(0, 0);
        var newLocation = new Location(1, 1);
        var timeout = TimeSpan.FromSeconds(1);
        var personLocationChangedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonLocationChangedEvent>();
        _testPerson = Person.Create(personId, initialLocation, timeout);

        // Act
        _testPerson.UpdateLocation(newLocation);
        var personLocationChangedEvent = await personLocationChangedEventTask;

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
        var personExpiredEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonExpiredEvent>();
        _testPerson = Person.Create(personId, location, lifespanTimeout);

        // Act
        var personExpiredEvent = await personExpiredEventTask;

        // Assert
        Assert.NotNull(personExpiredEvent);
        Assert.Equal(personId, personExpiredEvent.Person.Id);
    }

    public void Dispose()
    {
        _testPerson?.Dispose();
        _testPerson = null;

        DomainEvents.Dispose();
    }
}
