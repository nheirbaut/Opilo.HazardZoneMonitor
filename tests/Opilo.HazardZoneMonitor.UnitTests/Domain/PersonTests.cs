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
    public void Create_ShouldRaisePersonCreatedEvent_WhenParametersAreValid()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);
        var receivedEvents = new List<PersonCreatedEvent>();
        DomainEventDispatcher.Register<PersonCreatedEvent>(e => receivedEvents.Add(e));

        // Act
        _testPerson = Person.Create(personId, location, timeout);
        var personCreatedEvent = receivedEvents.Single();

        // Assert
        personCreatedEvent.Should().NotBeNull();
        personCreatedEvent.PersonId.Should().Be(personId);
        personCreatedEvent.Location.Should().Be(location);
    }

    [Fact]
    public void UpdateLocation_ShouldRaisePersonLocationChangedEvent_WhenLocationIsUpdatedToNewValue()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var initialLocation = new Location(0, 0);
        var newLocation = new Location(1, 1);
        var timeout = TimeSpan.FromSeconds(1);
        var receivedEvents = new List<PersonLocationChangedEvent>();
        DomainEventDispatcher.Register<PersonLocationChangedEvent>(e => receivedEvents.Add(e));

        _testPerson = Person.Create(personId, initialLocation, timeout);

        // Act
        _testPerson.UpdateLocation(newLocation);
        var personLocationChangedEvent = receivedEvents.Single();

        // Assert
        personLocationChangedEvent.Should().NotBeNull();
        personLocationChangedEvent.PersonId.Should().Be(personId);
        personLocationChangedEvent.CurrentLocation.Should().Be(newLocation);
    }

    [Fact]
    public void UpdateLocation_ShouldNotRaisePersonLocationChangedEvent_WhenLocationIsUpdatedToSameValue()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var testLocation = new Location(0, 0);
        var timeout = TimeSpan.FromSeconds(1);

        var receivedEvents = new List<PersonLocationChangedEvent>();
        DomainEventDispatcher.Register<PersonLocationChangedEvent>(e => receivedEvents.Add(e));
        _testPerson = Person.Create(personId, testLocation, timeout);

        // Act
        _testPerson.UpdateLocation(testLocation);
        var personLocationChangedEvent = receivedEvents.SingleOrDefault();

        // Assert
        personLocationChangedEvent.Should().BeNull();
    }

    [Fact]
    public void UpdateLocation_ShouldResetExpiration_WhenLocationIsUpdatedToSameValue()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var testLocation = new Location(0, 0);
        var timeout = TimeSpan.FromMilliseconds(100);

        var clock = new FakeClock(DateTime.UnixEpoch);
        var timerFactory = new FakeTimerFactory(clock);

        var expiredEvents = new List<PersonExpiredEvent>();
        DomainEventDispatcher.Register<PersonExpiredEvent>(e => expiredEvents.Add(e));

        _testPerson = Person.Create(personId, testLocation, timeout, clock, timerFactory);
        clock.AdvanceBy(TimeSpan.FromMilliseconds(75));

        // Act
        _testPerson.UpdateLocation(testLocation);
        clock.AdvanceBy(TimeSpan.FromMilliseconds(75)); // total 150ms from create

        // Assert
        expiredEvents.Should().BeEmpty();

        // And after enough time, it expires exactly once
        clock.AdvanceBy(TimeSpan.FromMilliseconds(30)); // total 180ms (> 175ms expected)
        expiredEvents.Should().ContainSingle();
        expiredEvents.Single().PersonId.Should().Be(personId);
    }

    [Fact]
    public void ExpirePerson_ShouldRaisePersonExpiredEvent_WhenTimeExpires()
    {
        // Arrange
        var lifespanTimeout = TimeSpan.FromMilliseconds(10);
        var personId = Guid.NewGuid();
        var location = new Location(0, 0);

        var clock = new FakeClock(DateTime.UnixEpoch);
        var timerFactory = new FakeTimerFactory(clock);

        var expiredEvents = new List<PersonExpiredEvent>();
        DomainEventDispatcher.Register<PersonExpiredEvent>(e => expiredEvents.Add(e));

        _testPerson = Person.Create(personId, location, lifespanTimeout, clock, timerFactory);

        // Act
        clock.AdvanceBy(lifespanTimeout);
        var personExpiredEvent = expiredEvents.Single();

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
