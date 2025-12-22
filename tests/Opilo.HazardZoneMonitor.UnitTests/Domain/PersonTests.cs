using Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;
using Opilo.HazardZoneMonitor.Shared.Primitives;

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
        Assert.Equal(personId, personCreatedEvent.PersonId);
        Assert.Equal(location, personCreatedEvent.Location);
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
        Assert.Equal(personId, personLocationChangedEvent.PersonId);
        Assert.Equal(newLocation, personLocationChangedEvent.CurrentLocation);
    }

    [Fact]
    public async Task UpdateLocation_WithSameLocation_DoesNotRaisePersonLocationChangedEvent()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var testLocation = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);

        var personLocationChangedEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonLocationChangedEvent>(TimeSpan.FromMilliseconds(20));
        _testPerson = Person.Create(personId, testLocation, timeout);

        // Act
        _testPerson.UpdateLocation(testLocation);
        var personLocationChangedEvent = await personLocationChangedEventTask;

        // Assert
        Assert.Null(personLocationChangedEvent);
    }

    [Fact]
    public async Task UpdateLocation_WithSameLocation_ResetsExpiration()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var testLocation = new Location(0, 0);
        var timeout = TimeSpan.FromMilliseconds(100);

        var personExpiredEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonExpiredEvent>(TimeSpan.FromMilliseconds(150));
        _testPerson = Person.Create(personId, testLocation, timeout);
        await Task.Delay(75);

        // Act
        _testPerson.UpdateLocation(testLocation);
        var personExpiredEvent = await personExpiredEventTask;

        // Assert
        Assert.Null(personExpiredEvent);
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
        Assert.Equal(personId, personExpiredEvent.PersonId);
    }

    public void Dispose()
    {
        _testPerson?.Dispose();
        _testPerson = null;

        DomainEventDispatcher.Dispose();
    }
}
