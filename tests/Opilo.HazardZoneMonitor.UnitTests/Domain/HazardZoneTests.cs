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
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new HazardZone(null!, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new HazardZone(invalidName, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero));
    }

    [Fact]
    public void Constructor_NullOutline_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HazardZone(HazardZoneBuilder.DefaultName, null!, TimeSpan.Zero));
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInactiveInstance()
    {
        // Act
        using var hazardZone =
            new HazardZone(HazardZoneBuilder.DefaultName, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero);

        // Assert
        Assert.Equal(HazardZoneBuilder.DefaultName, hazardZone.Name);
        Assert.Equal(HazardZoneBuilder.DefaultOutline, hazardZone.Outline);
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void Constructor_NegativePreAlarmDuration_HandlesAppropriately()
    {
        // Act
        using var hazardZone = new HazardZone(HazardZoneBuilder.DefaultName, HazardZoneBuilder.DefaultOutline,
            TimeSpan.FromMilliseconds(-100));

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(-100),
            hazardZone.PreAlarmDuration);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_PersonInZone_RaisesPersonAddedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personCreatedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.NotNull(personAddedToHazardZoneEvent);
        Assert.Equal(personCreatedEvent.PersonId, personAddedToHazardZoneEvent.PersonId);
        Assert.Equal(HazardZoneBuilder.DefaultName, personAddedToHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_PersonOutsideZone_DoesNotRaisePersonAddedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedOutsideHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions
                .RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromMilliseconds(50));

        // Act
        DomainEvents.Raise(personCreatedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.Null(personAddedToHazardZoneEvent);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_PersonInZone_RaisesPersonRemovedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        DomainEvents.Raise(personCreatedEvent);
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(new PersonExpiredEvent(personCreatedEvent.PersonId));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.NotNull(personRemovedFromHazardZoneEvent);
        Assert.Equal(personCreatedEvent.PersonId, personRemovedFromHazardZoneEvent.PersonId);
        Assert.Equal(HazardZoneBuilder.DefaultName, personRemovedFromHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_PersonNotInZone_DoesNotRaisePersonRemovedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedOutsideHazardZone(hazardZone);
        DomainEvents.Raise(personCreatedEvent);

        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>(
                TimeSpan.FromMilliseconds(40));

        // Act
        DomainEvents.Raise(new PersonExpiredEvent(personCreatedEvent.PersonId));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.Null(personRemovedFromHazardZoneEvent);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_UnknownPersonInZone_RaisesPersonAddedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personLocationChangedEvent = PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personLocationChangedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.NotNull(personAddedToHazardZoneEvent);
        Assert.Equal(personLocationChangedEvent.PersonId, personAddedToHazardZoneEvent.PersonId);
        Assert.Equal(HazardZoneBuilder.DefaultName, personAddedToHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_UnknownPersonOutsideZone_DoesNotRaisePersonAddedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedOutsideHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromMilliseconds(40));

        // Act
        DomainEvents.Raise(personLocationChangedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.Null(personAddedToHazardZoneEvent);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_KnownPersonMovesOutsideZone_RaisesPersonRemovedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var initialPersonLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);
        DomainEvents.Raise(initialPersonLocationChangedEvent);

        var newPersonLocationChangedEvent = PersonHelper.CreatePersonLocationChangedEventLocatedOutsideHazardZone(
            hazardZone,
            initialPersonLocationChangedEvent.PersonId);

        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(newPersonLocationChangedEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.NotNull(personRemovedFromHazardZoneEvent);
        Assert.Equal(initialPersonLocationChangedEvent.PersonId, personRemovedFromHazardZoneEvent.PersonId);
        Assert.Equal(HazardZoneBuilder.DefaultName, personRemovedFromHazardZoneEvent.HazardZoneName);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_KnownPersonMovesWithinZone_DoesNotRaiseEvents()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var initialPersonLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);

        var initialPersonLocationChangedEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonLocationChangedEvent>();

        DomainEvents.Raise(initialPersonLocationChangedEvent);
        await initialPersonLocationChangedEventTask;

        var newLocation = new Location(3, 3);
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>(
                TimeSpan.FromMilliseconds(40));

        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromMilliseconds(40));

        // Act
        DomainEvents.Raise(new PersonLocationChangedEvent(initialPersonLocationChangedEvent.PersonId, newLocation));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        Assert.Null(personRemovedFromHazardZoneEvent);
        Assert.Null(personAddedToHazardZoneEvent);
    }

    [Fact]
    public void ActivateFromExternalSource_NullSourceId_ThrowsArgumentNullException()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => hazardZone.ActivateFromExternalSource(null!));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void ActivateFromExternalSource_InvalidSourceId_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => hazardZone.ActivateFromExternalSource(invalidName));
    }

    [Fact]
    public void DeactivateFromExternalSource_NullSourceId_ThrowsArgumentNullException()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => hazardZone.DeactivateFromExternalSource(null!));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void DeactivateFromExternalSource_InvalidSourceId_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => hazardZone.DeactivateFromExternalSource(invalidName));
    }

    //------------------------------------------------------------------------------
    // Inactive (IsActive=false, AlarmState=None)
    //------------------------------------------------------------------------------

    [Fact]
    public void ManuallyActivate_InactiveState_TransitionsToActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void ActivateFromExternalSource_InactiveStateWithUnknownSource_TransitionsToActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act
        hazardZone.ActivateFromExternalSource("ext-src");

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void ActivateFromExternalSource_InactiveStateWithKnownSource_DoesNotTransition()
    {
        // Arrange
        var sourceId = "ext-src";
        using var hazardZone = HazardZoneBuilder.Create()
            .WithExternalActivationSource(sourceId)
            .Build();

        // Act
        hazardZone.ActivateFromExternalSource(sourceId);

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void ManuallyDeactivate_InactiveState_RemainsInactive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act
        hazardZone.ManuallyDeactivate();

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    //------------------------------------------------------------------------------
    // Active (IsActive=true, AlarmState=None)
    //------------------------------------------------------------------------------

    [Fact]
    public async Task OnPersonCreatedEvent_ActiveStateUnderThreshold_RemainsActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithAllowedNumberOfPersons(1)
            .WithState(HazardZoneTestState.Active)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task SetAllowedNumberOfPersons_ActiveStateAboveThreshold_RemainsActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(3)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();
        DomainEvents.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Act
        hazardZone.SetAllowedNumberOfPersons(2);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_ActiveStateOverThresholdWithPreAlarm_TransitionsToPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
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
    public async Task SetAllowedNumberOfPersons_ActiveStateBelowThresholdWithPreAlarm_TransitionsToPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
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
    public async Task OnPersonCreatedEvent_ActiveStateOverThresholdWithZeroPreAlarm_TransitionsToAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithPreAlarmDuration(TimeSpan.Zero)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public async Task SetAllowedNumberOfPersons_ActiveStateBelowThresholdWithZeroPreAlarm_TransitionsToAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .WithPreAlarmDuration(TimeSpan.Zero)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();
        DomainEvents.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public void ManuallyDeactivate_ActiveState_TransitionsToInactive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .Build();

        // Act
        hazardZone.ManuallyDeactivate();

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void DeactivateFromExternalSource_ActiveStateWithKnownSource_TransitionsToInactive()
    {
        // Arrange
        var sourceId = "ext-src";
        using var hazardZone = HazardZoneBuilder.Create()
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
    public void DeactivateFromExternalSource_ActiveStateWithUnknownSource_RemainsActive()
    {
        // Arrange
        var sourceId1 = "ext-src1";
        using var hazardZone = HazardZoneBuilder.Create()
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

    [Fact]
    public void ActivateFromExternalSource_ActiveStateWithUnknownSource_AddsSourceAndRemainsActive()
    {
        // Arrange
        var sourceId = "ext-src";
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .Build();

        // Act
        hazardZone.ActivateFromExternalSource(sourceId);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void ActivateFromExternalSource_ActiveStateWithKnownSource_RemainsActiveWithoutAddingSource()
    {
        // Arrange
        var sourceId = "ext-src";
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithExternalActivationSource(sourceId)
            .Build();

        // Act
        hazardZone.ActivateFromExternalSource(sourceId);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ActiveStatePersonMovesOutsideUnderThreshold_RemainsActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .Build();
        var personId = Guid.NewGuid();
        var initialEvent = new PersonCreatedEvent(personId, new Location(2, 2));
        DomainEvents.Raise(initialEvent);
        await DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        var moveEvent = new PersonLocationChangedEvent(personId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(moveEvent);
        await removedEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    //------------------------------------------------------------------------------
    // PreAlarm (IsActive=true, AlarmState=PreAlarm)
    //------------------------------------------------------------------------------

    [Fact]
    public async Task OnPersonExpiredEvent_PreAlarmStateOverThreshold_RemainsInPreAlarm()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var secondPersonAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();
        DomainEvents.Raise(new PersonCreatedEvent(Guid.NewGuid(), new Location(2, 2)));

        var secondPersonAddedToHazardZoneEvent = await secondPersonAddedToHazardZoneEventTask;
        Assert.NotNull(secondPersonAddedToHazardZoneEvent);

        var firstPersonExpiredEvent = new PersonExpiredEvent(hazardZoneBuilder.IdsOfPersonsAdded.First());
        var firstPersonRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(firstPersonExpiredEvent);
        var personRemovedFromHazardZoneEvent = await firstPersonRemovedFromHazardZoneEventTask;

        // Assert
        Assert.NotNull(personRemovedFromHazardZoneEvent);
        Assert.Equal(firstPersonExpiredEvent.PersonId, personRemovedFromHazardZoneEvent.PersonId);
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.PreAlarm, hazardZone.AlarmState);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_PreAlarmStateBelowThreshold_RemainsInPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithAllowedNumberOfPersons(1)
            .WithState(HazardZoneTestState.PreAlarm)
            .Build();

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.PreAlarm, hazardZone.AlarmState);
    }

    [Fact]
    public void DeactivateFromExternalSource_PreAlarmStateWithUnknownSource_DoesNotDeactivate()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .Build();

        // Act
        hazardZone.DeactivateFromExternalSource("ext-src");

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.PreAlarm, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_PreAlarmStateUnderThreshold_TransitionsToActive()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var personExpiredEvent = new PersonExpiredEvent(hazardZoneBuilder.IdsOfPersonsAdded.First());
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personExpiredEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.NotNull(personRemovedFromHazardZoneEvent);
        Assert.Equal(personExpiredEvent.PersonId, personRemovedFromHazardZoneEvent.PersonId);
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_PreAlarmStateEqualToCount_TransitionsToActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .Build();

        // Act
        hazardZone.SetAllowedNumberOfPersons(1);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPreAlarmTimerElapsed_PreAlarmState_TransitionsToAlarm()
    {
        var testPreAlarmDuration = TimeSpan.FromMilliseconds(10);

        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithPreAlarmDuration(testPreAlarmDuration)
            .Build();

        // Act
        await Task.Delay(testPreAlarmDuration * 4);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public void ManuallyDeactivate_PreAlarmState_TransitionsToInactive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .Build();

        // Act
        hazardZone.ManuallyDeactivate();

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void DeactivateFromExternalSource_PreAlarmStateWithKnownSource_TransitionsToInactive()
    {
        // Arrange
        var sourceId = "ext-src";
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithExternalActivationSource(sourceId)
            .Build();

        // Act
        hazardZone.DeactivateFromExternalSource(sourceId);

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_PreAlarmStateOverThreshold_RemainsInPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(1)
            .Build();
        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
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
    public async Task OnPersonLocationChangedEvent_PreAlarmStatePersonMovesOutsideUnderThreshold_TransitionsToActive()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(1);
        using var hazardZone = hazardZoneBuilder.Build();
        var personId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var moveEvent = new PersonLocationChangedEvent(personId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(moveEvent);
        await removedEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_PreAlarmStatePersonMovesOutsideOverThreshold_RemainsInPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(0)
            .Build();
        var additionalPersonId = Guid.NewGuid();
        DomainEvents.Raise(new PersonCreatedEvent(additionalPersonId, new Location(2, 2)));
        await DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        var moveEvent = new PersonLocationChangedEvent(additionalPersonId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(moveEvent);
        await removedEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.PreAlarm, hazardZone.AlarmState);
    }

    [Fact]
    public void ManuallyActivate_PreAlarmState_RemainsInPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .Build();

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.PreAlarm, hazardZone.AlarmState);
    }

    //------------------------------------------------------------------------------
    // Alarm (IsActive=true, AlarmState=Alarm)
    //------------------------------------------------------------------------------

    [Fact]
    public async Task OnPersonExpiredEvent_AlarmStateOverThreshold_RemainsInAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .Build();

        var newPersonId = Guid.NewGuid();
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();
        DomainEvents.Raise(new PersonCreatedEvent(newPersonId, new Location(2, 2)));

        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;
        Assert.NotNull(personAddedToHazardZoneEvent);
        Assert.Equal(newPersonId, personAddedToHazardZoneEvent.PersonId);

        var personExpiredEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonExpiredEvent>();

        // Act
        DomainEvents.Raise(new PersonExpiredEvent(newPersonId));
        await personExpiredEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_AlarmStateBelowThreshold_RemainsInAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithAllowedNumberOfPersons(1)
            .WithState(HazardZoneTestState.Alarm)
            .Build();

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public void DeactivateFromExternalSource_AlarmStateWithUnknownSource_DoesNotDeactivate()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .Build();

        // Act
        hazardZone.DeactivateFromExternalSource("ext-src");

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_AlarmStateUnderThreshold_TransitionsToActive()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var personExpiredEvent = new PersonExpiredEvent(hazardZoneBuilder.IdsOfPersonsAdded.First());
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personExpiredEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        Assert.NotNull(personRemovedFromHazardZoneEvent);
        Assert.Equal(personExpiredEvent.PersonId, personRemovedFromHazardZoneEvent.PersonId);
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_AlarmStateEqualToCount_TransitionsToActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .Build();

        // Act
        hazardZone.SetAllowedNumberOfPersons(1);

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void ManuallyDeactivate_AlarmState_TransitionsToInactive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .Build();

        // Act
        hazardZone.ManuallyDeactivate();

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public void DeactivateFromExternalSource_AlarmStateWithKnownSource_TransitionsToInactive()
    {
        // Arrange
        var sourceId = "ext-src";
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithExternalActivationSource(sourceId)
            .Build();

        // Act
        hazardZone.DeactivateFromExternalSource(sourceId);

        // Assert
        Assert.False(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_AlarmStateOverThreshold_RemainsInAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(1)
            .Build();
        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        // Act
        DomainEvents.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_AlarmStatePersonMovesOutsideUnderThreshold_TransitionsToActive()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(1);
        using var hazardZone = hazardZoneBuilder.Build();
        var personId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var moveEvent = new PersonLocationChangedEvent(personId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(moveEvent);
        await removedEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.None, hazardZone.AlarmState);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_AlarmStatePersonMovesOutsideOverThreshold_RemainsInAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(0)
            .Build();
        var additionalPersonId = Guid.NewGuid();
        DomainEvents.Raise(new PersonCreatedEvent(additionalPersonId, new Location(2, 2)));
        await DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        var moveEvent = new PersonLocationChangedEvent(additionalPersonId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEvents.Raise(moveEvent);
        await removedEventTask;

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public void ManuallyActivate_AlarmState_RemainsInAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .Build();

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        Assert.True(hazardZone.IsActive);
        Assert.Equal(AlarmState.Alarm, hazardZone.AlarmState);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_NegativeValue_IgnoresChange()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .Build();

        // Act
        hazardZone.SetAllowedNumberOfPersons(-1);

        // Assert
        Assert.Equal(1, hazardZone.AllowedNumberOfPersons);
    }

    public void Dispose()
    {
        DomainEvents.Dispose();
    }
}
