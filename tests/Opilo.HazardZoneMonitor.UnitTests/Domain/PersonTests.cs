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
    public void Create_ShouldCreateValidPerson_WhenParametersAreValid()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);

        // Act
        _testPerson = Person.Create(personId, location, timeout);

        // Assert
        _testPerson.Should().NotBeNull();
        _testPerson!.Id.Should().Be(personId);
        _testPerson.Location.Should().Be(location);
    }

    [Fact]
    public async Task Create_ShouldRaisePersonCreatedEvent_WhenParametersAreValid()
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
        personCreatedEvent.Should().NotBeNull();
        personCreatedEvent.PersonId.Should().Be(personId);
        personCreatedEvent.Location.Should().Be(location);
    }

    [Fact]
    public async Task UpdateLocation_ShouldRaisePersonLocationChangedEvent_WhenLocationIsUpdatedToNewValue()
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
        personLocationChangedEvent.Should().NotBeNull();
        personLocationChangedEvent.PersonId.Should().Be(personId);
        personLocationChangedEvent.CurrentLocation.Should().Be(newLocation);
    }

    [Fact]
    public async Task UpdateLocation_ShouldNotRaisePersonLocationChangedEvent_WhenLocationIsUpdatedToSameValue()
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
        personLocationChangedEvent.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLocation_ShouldResetExpiration_WhenLocationIsUpdatedToSameValue()
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
        personExpiredEvent.Should().BeNull();
    }

    [Fact]
    public async Task ExpirePerson_ShouldRaisePersonExpiredEvent_WhenTimeExpires()
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
        personExpiredEvent.Should().NotBeNull();
        personExpiredEvent.PersonId.Should().Be(personId);
    }

    public void Dispose()
    {
        _testPerson?.Dispose();
        _testPerson = null;

        DomainEventDispatcher.Dispose();
    }
}
