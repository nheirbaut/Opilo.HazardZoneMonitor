using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class HazardZoneTests : IDisposable
{
    private static readonly Outline s_validOutline = new(new([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]));

    private const string ValidHazardZoneName = "TestHazardZone";

    [Fact]
    public void Constructor_WhenNameIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HazardZone(null!, s_validOutline));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_WhenNameIsInvalid_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HazardZone(invalidName, s_validOutline));
    }

    [Fact]
    public void Constructor_WhenOutlineIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HazardZone(ValidHazardZoneName, null!));
    }

    [Fact]
    public void Constructor_WhenValidNameAndOutlineGiven_CreatesInstance()
    {
        // Act
        var hazardZone = new HazardZone(ValidHazardZoneName, s_validOutline);

        // Assert
        Assert.Equal(ValidHazardZoneName, hazardZone.Name);
        Assert.Equal(s_validOutline, hazardZone.Outline);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_WhenPersonCreatedIsLocatedInHazardZone_RaisesPersonAddedToHazardZoneEvent()
    {
        // Arrange
        _ = new HazardZone(ValidHazardZoneName, s_validOutline);
        var personId = Guid.NewGuid();
        var initialLocation = new Location(2, 2);
        var personCreatedEvent = new PersonCreatedEvent(personId, initialLocation);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personCreatedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.NotNull(personAddedToHazardZoneEvent);
        Assert.Equal(personId, personAddedToHazardZoneEvent.PersonId);
        Assert.Equal(ValidHazardZoneName, personAddedToHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task
        OnPersonCreatedEvent_WhenPersonCreatedIsNotLocatedInHazardZone_DoesNotRaisePersonAddedToHazardZoneEvent()
    {
        // Arrange
        _ = new HazardZone(ValidHazardZoneName, s_validOutline);
        var personId = Guid.NewGuid();
        var initialLocation = new Location(8, 8);
        var personCreatedEvent = new PersonCreatedEvent(personId, initialLocation);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromMilliseconds(50));

        // Act
        DomainEvents.Raise(personCreatedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.Null(personAddedToHazardZoneEvent);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_WhenPersonIsInZone_RaisesPersonRemovedFromHazardZoneEvent()
    {
        // Arrange
        _ = new HazardZone(ValidHazardZoneName, s_validOutline);
        var personId = Guid.NewGuid();
        var initialLocation = new Location(2, 2);
        DomainEvents.Raise(new PersonCreatedEvent(personId, initialLocation));
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(new PersonExpiredEvent(personId));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.NotNull(personRemovedFromHazardZoneEvent);
        Assert.Equal(personId, personRemovedFromHazardZoneEvent.PersonId);
        Assert.Equal(ValidHazardZoneName, personRemovedFromHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_WhenPersonIsNotInZone_RaisesPersonRemovedFromHazardZoneEvent()
    {
        // Arrange
        _ = new HazardZone(ValidHazardZoneName, s_validOutline);
        var personId = Guid.NewGuid();
        var initialLocation = new Location(8, 8);
        DomainEvents.Raise(new PersonCreatedEvent(personId, initialLocation));
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>(
                TimeSpan.FromMilliseconds(40));

        // Act
        DomainEvents.Raise(new PersonExpiredEvent(personId));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.Null(personRemovedFromHazardZoneEvent);
    }

    [Fact]
    public async Task PersonLocationChangedEvent_WhenPersonNotKnownAndLocationUpdateInZone_RaisesPersonAddedToHazardZoneEvent()
    {
        // Arrange
        _ = new HazardZone(ValidHazardZoneName, s_validOutline);
        var personId = Guid.NewGuid();
        var initialLocation = new Location(2, 2);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        // Act
        DomainEvents.Raise(new PersonLocationChangedEvent(personId, initialLocation));
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.NotNull(personAddedToHazardZoneEvent);
        Assert.Equal(personId, personAddedToHazardZoneEvent.PersonId);
        Assert.Equal(ValidHazardZoneName, personAddedToHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task
        PersonLocationChangedEvent_WhenPersonNotKnownAndLocationUpdateNotInZone_DoesNotRaisePersonAddedToHazardZoneEvent()
    {
        // Arrange
        _ = new HazardZone(ValidHazardZoneName, s_validOutline);
        var personId = Guid.NewGuid();
        var initialLocation = new Location(8, 8);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromMilliseconds(40));

        // Act
        DomainEvents.Raise(new PersonLocationChangedEvent(personId, initialLocation));
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.Null(personAddedToHazardZoneEvent);
    }

    [Fact]
    public async Task PersonLocationChangedEvent_WhenPersonKnownAndLocationUpdateNotInZone_RaisesPersonRemovedFromHazardZoneEvent()
    {
        // Arrange
        _ = new HazardZone(ValidHazardZoneName, s_validOutline);
        var personId = Guid.NewGuid();
        var initialLocation = new Location(2, 2);
        var newLocation = new Location(8, 8);
        DomainEvents.Raise(new PersonLocationChangedEvent(personId, initialLocation));
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(new PersonLocationChangedEvent(personId, newLocation));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.NotNull(personRemovedFromHazardZoneEvent);
        Assert.Equal(personId, personRemovedFromHazardZoneEvent.PersonId);
        Assert.Equal(ValidHazardZoneName, personRemovedFromHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task
        PersonLocationChangedEvent_WhenPersonKnownAndLocationUpdateInZone_DoesNotRaisePersonAddedToHazardZoneEventOrPersonRemovedFromHazardZoneEvent()
    {
        // Arrange
        _ = new HazardZone(ValidHazardZoneName, s_validOutline);
        var personId = Guid.NewGuid();
        var initialLocation = new Location(2, 2);
        var newLocation = new Location(3, 3);
        var initialPersonLocationChangedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonLocationChangedEvent>();
        DomainEvents.Raise(new PersonLocationChangedEvent(personId, initialLocation));
        await initialPersonLocationChangedEventTask;
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>(
                TimeSpan.FromMilliseconds(40));
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromMilliseconds(40));

        // Act
        DomainEvents.Raise(new PersonLocationChangedEvent(personId, newLocation));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.Null(personRemovedFromHazardZoneEvent);
        Assert.Null(personAddedToHazardZoneEvent);
    }

    [Fact]
    public async Task Activate_WhenHazardZoneIsNotActive_RaisesHazardZoneActivationStartedEvent()
    {
        // Arrange
        var hazardZone = new HazardZone(ValidHazardZoneName, s_validOutline);
        var hazardZoneActivationStartedEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<HazardZoneActivationStartedEvent>();

        // Act
        hazardZone.Activate();
        var hazardZoneActivationStartedEvent = await hazardZoneActivationStartedEventTask;

        // Assert
        Assert.NotNull(hazardZoneActivationStartedEvent);
        Assert.Equal(ValidHazardZoneName, hazardZoneActivationStartedEvent.HazardZoneName);
    }

    public void Dispose()
    {
        DomainEvents.Dispose();
    }
}
