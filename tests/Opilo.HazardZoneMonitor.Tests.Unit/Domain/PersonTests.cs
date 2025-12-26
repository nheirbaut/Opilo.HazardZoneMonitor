using Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Tests.Unit.TestUtilities;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Tests.Unit.Domain;

public sealed class PersonTests : IDisposable
{
    Person? _testPerson;
    private readonly FakeClock _clock;
    private readonly FakeTimerFactory _timerFactory;
    private readonly Guid _personId;
    private readonly Location _location;
    private readonly TimeSpan _timeout;

    public PersonTests()
    {
        _personId = Guid.NewGuid();
        _location = new Location(0, 0);
        _timeout = TimeSpan.FromSeconds(1);
        _clock = new FakeClock(DateTime.UnixEpoch);
        _timerFactory = new FakeTimerFactory(_clock);
    }

    [Fact]
    public void Create_ShouldCreateValidPerson_WhenParametersAreValid()
    {
        // Act
        _testPerson = Person.Create(_personId, _location, _timeout, _clock, _timerFactory);

        // Assert
        _testPerson.Should().NotBeNull();
        _testPerson!.Id.Should().Be(_personId);
        _testPerson.Location.Should().Be(_location);
    }

    [Fact]
    public void UpdateLocation_ShouldRaisePersonLocationChangedEvent_WhenLocationIsUpdatedToNewValue()
    {
        // Arrange
        var newLocation = new Location(1, 1);
        PersonLocationChangedEventArgs? personLocationChangedEvent = null;

        _testPerson = Person.Create(_personId, _location, _timeout, _clock, _timerFactory);
        _testPerson.LocationChanged += (_, e) => personLocationChangedEvent = e;

        // Act
        _testPerson.UpdateLocation(newLocation);

        // Assert
        personLocationChangedEvent.Should().NotBeNull();
        personLocationChangedEvent.PersonId.Should().Be(_personId);
        personLocationChangedEvent.CurrentLocation.Should().Be(newLocation);
    }

    [Fact]
    public void UpdateLocation_ShouldNotRaisePersonLocationChangedEvent_WhenLocationIsUpdatedToSameValue()
    {
        // Arrange
        PersonLocationChangedEventArgs? personLocationChangedEvent = null;

        _testPerson = Person.Create(_personId, _location, _timeout, _clock, _timerFactory);
        _testPerson.LocationChanged += (_, e) => personLocationChangedEvent = e;

        // Act
        _testPerson.UpdateLocation(_location);

        // Assert
        personLocationChangedEvent.Should().BeNull();
    }

    [Fact]
    public void UpdateLocation_ShouldResetExpiration_WhenLocationIsUpdatedToSameValue()
    {
        // Arrange
        var clock = new FakeClock(DateTime.UnixEpoch);
        var timerFactory = new FakeTimerFactory(clock);

        var expiredEvents = new List<PersonExpiredEventArgs>();

        _testPerson = Person.Create(_personId, _location, _timeout, clock, timerFactory);
        _testPerson.Expired += (_, e) => expiredEvents.Add(e);
        clock.AdvanceBy(_timeout / 2);

        // Act
        _testPerson.UpdateLocation(_location);
        clock.AdvanceBy(_timeout - TimeSpan.FromMilliseconds(1));

        // Assert
        expiredEvents.Should().BeEmpty();
    }

    [Fact]
    public void ExpirePerson_ShouldRaisePersonExpiredEvent_WhenTimeExpires()
    {
        // Arrange
        var expiredEvents = new List<PersonExpiredEventArgs>();

        _testPerson = Person.Create(_personId, _location, _timeout, _clock, _timerFactory);
        _testPerson.Expired += (_, e) => expiredEvents.Add(e);

        // Act
        _clock.AdvanceBy(_timeout);
        var personExpiredEvent = expiredEvents.Single();

        // Assert
        personExpiredEvent.Should().NotBeNull();
        personExpiredEvent.PersonId.Should().Be(_personId);
    }

    public void Dispose()
    {
        _testPerson?.Dispose();
        _testPerson = null;
    }
}
