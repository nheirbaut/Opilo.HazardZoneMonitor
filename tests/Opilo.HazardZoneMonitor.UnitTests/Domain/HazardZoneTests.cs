using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.Enums;
using Opilo.HazardZoneMonitor.Domain.Events.HazardZoneEvents;
using Opilo.HazardZoneMonitor.Domain.Events.PersonEvents;
using Opilo.HazardZoneMonitor.Domain.Services;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class HazardZoneTests : IDisposable
{
    [Fact]
    public void Constructor_WhenNameIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HazardZone(null!, HazardZoneBuilder.DefaultOutline));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_WhenNameIsInvalid_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HazardZone(invalidName, HazardZoneBuilder.DefaultOutline));
    }

    [Fact]
    public void Constructor_WhenOutlineIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HazardZone(HazardZoneBuilder.DefaultName, null!));
    }

    [Fact]
    public void Constructor_WhenValidNameAndOutlineGiven_CreatesInactiveInstance()
    {
        // Act
        var hazardZone = new HazardZone(HazardZoneBuilder.DefaultName, HazardZoneBuilder.DefaultOutline);

        // Assert
        Assert.Equal(HazardZoneBuilder.DefaultName, hazardZone.Name);
        Assert.Equal(HazardZoneBuilder.DefaultOutline, hazardZone.Outline);
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_WhenPersonCreatedIsLocatedInHazardZone_RaisesPersonAddedToHazardZoneEvent()
    {
        // Arrange
        _ = HazardZoneBuilder.BuildSimple();
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
        Assert.Equal(HazardZoneBuilder.DefaultName, personAddedToHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task
        OnPersonCreatedEvent_WhenPersonCreatedIsNotLocatedInHazardZone_DoesNotRaisePersonAddedToHazardZoneEvent()
    {
        // Arrange
        _ = HazardZoneBuilder.BuildSimple();
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
        _ = HazardZoneBuilder.BuildSimple();
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
        Assert.Equal(HazardZoneBuilder.DefaultName, personRemovedFromHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_WhenPersonIsNotInZone_RaisesPersonRemovedFromHazardZoneEvent()
    {
        // Arrange
        _ = HazardZoneBuilder.BuildSimple();
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
        _ = HazardZoneBuilder.BuildSimple();
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
        Assert.Equal(HazardZoneBuilder.DefaultName, personAddedToHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task
        PersonLocationChangedEvent_WhenPersonNotKnownAndLocationUpdateNotInZone_DoesNotRaisePersonAddedToHazardZoneEvent()
    {
        // Arrange
        _ = HazardZoneBuilder.BuildSimple();
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
        _ = HazardZoneBuilder.BuildSimple();
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
        Assert.Equal(HazardZoneBuilder.DefaultName, personRemovedFromHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task
        PersonLocationChangedEvent_WhenPersonKnownAndLocationUpdateInZone_DoesNotRaisePersonAddedToHazardZoneEventOrPersonRemovedFromHazardZoneEvent()
    {
        // Arrange
        _ = HazardZoneBuilder.BuildSimple();
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
    public void ActivateFromExternalSource_WhenNameNull_ThrowsArgumentNullException()
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => hazardZone.ActivateFromExternalSource(null!));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void ActivateFromExternalSource_WhenNameIsInvalid_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => hazardZone.ActivateFromExternalSource(invalidName));
    }

    [Fact]
    public void DeactivateFromExternalSource_WhenNameNull_ThrowsArgumentNullException()
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => hazardZone.DeactivateFromExternalSource(null!));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void DeactivateFromExternalSource_WhenNameIsInvalid_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => hazardZone.DeactivateFromExternalSource(invalidName));
    }

    //------------------------------------------------------------------------------
    // Inactive (IsActive=false, AlarmState=None)
    //------------------------------------------------------------------------------

    [Fact]
    public void ManuallyActivate_WhenStateIsInactive_ActivatesTheHazardZone()
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void ActivateFromExternalSource_WhenStateIsInactiveAndSourceIdUnknown_ActivatesTheHazardZone()
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act
        hazardZone.ActivateFromExternalSource("ext-src");

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void ActivateFromExternalSource_WhenStateIsInactiveAndSourceIdKnown_DoesNotActivateTheHazardZone()
    {
        // Arrange
        var sourceId = "ext-src";
        var hazardZone = HazardZoneBuilder.Create()
            .WithExternalActivationSource(sourceId)
            .Build();

        // Act
        hazardZone.ActivateFromExternalSource(sourceId);

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    //------------------------------------------------------------------------------
    // Active (IsActive=true, AlarmState=None)
    //------------------------------------------------------------------------------

    [Fact]
    public async Task AddPerson_WhenStateIsActiveAndThresholdIsZero_SetsPreAlarm()
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .Build();

        var personId = Guid.NewGuid();
        var initialLocation = new Location(2, 2);
        var personCreatedEvent = new PersonCreatedEvent(personId, initialLocation);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.PreAlarm, hazardZone.AlarmState);
    }

    [Fact]
    public async Task SetAllowedNumberOfPersons_WhenStateIsActiceAndAllowedIsOneLessThanCurrentlyInZone_SetsPreAlarm()
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .Build();

        var personId = Guid.NewGuid();
        var initialLocation = new Location(2, 2);
        var personCreatedEvent = new PersonCreatedEvent(personId, initialLocation);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();
        DomainEvents.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.PreAlarm, hazardZone.AlarmState);
    }

    [Fact]
    public void ManuallyDeactivate_WhenStateIsActive_DeactivatesTheHazardZone()
    {
        // Arrange
        var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .Build();

        // Act
        hazardZone.ManuallyDeactivate();

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void DeactivateFromExternalSource_WhenStateIsActiveAndSourceIdKnown_DeactivatesTheHazardZone()
    {
        // Arrange
        var sourceId = "ext-src";
        var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithExternalActivationSource(sourceId)
            .Build();

        // Act
        hazardZone.DeactivateFromExternalSource(sourceId);

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void DeactivateFromExternalSource_WhenStateIsInactiveAndSourceIdUnknown_DoesNotDeactivateTheHazardZone()
    {
        // Arrange
        var sourceId1 = "ext-src1";
        var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithExternalActivationSource(sourceId1)
            .Build();

        var sourceId2 = "ext-src2";

        // Act
        hazardZone.DeactivateFromExternalSource(sourceId2);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    //------------------------------------------------------------------------------
    // PreAlarm (IsActive=true, AlarmState=PreAlarm)
    //------------------------------------------------------------------------------

    [Fact]
    public async Task RemovePerson_WhenStateIsPreAlarm_SetsZoneAsActiveAndAlarmStateNone()
    {
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();
        var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .Build();
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;
        Assert.NotNull(personAddedToHazardZoneEvent);
        var personExpiredEvent = new PersonExpiredEvent(personAddedToHazardZoneEvent.PersonId);
        var personRemovedFromHazardZoneEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personExpiredEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.NotNull(personRemovedFromHazardZoneEvent);
        Assert.Equal(personExpiredEvent.PersonId, personRemovedFromHazardZoneEvent.PersonId);
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    //------------------------------------------------------------------------------
    // Alarm (IsActive=true, AlarmState=Alarm)
    //------------------------------------------------------------------------------

    public void Dispose()
    {
        DomainEvents.Dispose();
    }
}
