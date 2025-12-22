using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Domain;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class HazardZoneTests : IDisposable
{
    [Fact]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new HazardZone(null!, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        var act = () => new HazardZone(invalidName, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NullOutline_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new HazardZone(HazardZoneBuilder.DefaultName, null!, TimeSpan.Zero);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInactiveInstance()
    {
        // Act
        using var hazardZone =
            new HazardZone(HazardZoneBuilder.DefaultName, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero);

        // Assert
        hazardZone.Name.Should().Be(HazardZoneBuilder.DefaultName);
        hazardZone.Outline.Should().Be(HazardZoneBuilder.DefaultOutline);
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void Constructor_NegativePreAlarmDuration_HandlesAppropriately()
    {
        // Act
        using var hazardZone = new HazardZone(HazardZoneBuilder.DefaultName, HazardZoneBuilder.DefaultOutline,
            TimeSpan.FromMilliseconds(-100));

        // Assert
        hazardZone.PreAlarmDuration.Should().Be(TimeSpan.FromMilliseconds(-100));
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().NotBeNull();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personCreatedEvent.PersonId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().BeNull();
    }

    [Fact]
    public async Task OnPersonExpiredEvent_PersonInZone_RaisesPersonRemovedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        DomainEventDispatcher.Raise(personCreatedEvent);
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEventDispatcher.Raise(new PersonExpiredEvent(personCreatedEvent.PersonId));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personCreatedEvent.PersonId);
        personRemovedFromHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_PersonNotInZone_DoesNotRaisePersonRemovedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedOutsideHazardZone(hazardZone);
        DomainEventDispatcher.Raise(personCreatedEvent);

        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>(
                TimeSpan.FromMilliseconds(40));

        // Act
        DomainEventDispatcher.Raise(new PersonExpiredEvent(personCreatedEvent.PersonId));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().BeNull();
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
        DomainEventDispatcher.Raise(personLocationChangedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().NotBeNull();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personLocationChangedEvent.PersonId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
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
        DomainEventDispatcher.Raise(personLocationChangedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().BeNull();
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_KnownPersonMovesOutsideZone_RaisesPersonRemovedEvent()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var initialPersonLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);
        DomainEventDispatcher.Raise(initialPersonLocationChangedEvent);

        var newPersonLocationChangedEvent = PersonHelper.CreatePersonLocationChangedEventLocatedOutsideHazardZone(
            hazardZone,
            initialPersonLocationChangedEvent.PersonId);

        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEventDispatcher.Raise(newPersonLocationChangedEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(initialPersonLocationChangedEvent.PersonId);
        personRemovedFromHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
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

        DomainEventDispatcher.Raise(initialPersonLocationChangedEvent);
        await initialPersonLocationChangedEventTask;

        var newLocation = new Location(3, 3);
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>(
                TimeSpan.FromMilliseconds(40));

        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromMilliseconds(40));

        // Act
        DomainEventDispatcher.Raise(new PersonLocationChangedEvent(initialPersonLocationChangedEvent.PersonId, newLocation));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().BeNull();
        personAddedToHazardZoneEvent.Should().BeNull();
    }

    [Fact]
    public void ActivateFromExternalSource_NullSourceId_ThrowsArgumentNullException()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        var act = () => hazardZone.ActivateFromExternalSource(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void ActivateFromExternalSource_InvalidSourceId_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        var act = () => hazardZone.ActivateFromExternalSource(invalidName);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeactivateFromExternalSource_NullSourceId_ThrowsArgumentNullException()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        var act = () => hazardZone.DeactivateFromExternalSource(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void DeactivateFromExternalSource_InvalidSourceId_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        var act = () => hazardZone.DeactivateFromExternalSource(invalidName);
        act.Should().Throw<ArgumentException>();
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void ActivateFromExternalSource_InactiveStateWithUnknownSource_TransitionsToActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act
        hazardZone.ActivateFromExternalSource("ext-src");

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void ManuallyDeactivate_InactiveState_RemainsInactive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act
        hazardZone.ManuallyDeactivate();

        // Assert
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Act
        hazardZone.SetAllowedNumberOfPersons(2);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(initialEvent);
        await DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        var moveEvent = new PersonLocationChangedEvent(personId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEventDispatcher.Raise(moveEvent);
        await removedEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(new PersonCreatedEvent(Guid.NewGuid(), new Location(2, 2)));

        var secondPersonAddedToHazardZoneEvent = await secondPersonAddedToHazardZoneEventTask;
        secondPersonAddedToHazardZoneEvent.Should().NotBeNull();

        var firstPersonExpiredEvent = new PersonExpiredEvent(hazardZoneBuilder.IdsOfPersonsAdded.First());
        var firstPersonRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEventDispatcher.Raise(firstPersonExpiredEvent);
        var personRemovedFromHazardZoneEvent = await firstPersonRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(firstPersonExpiredEvent.PersonId);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
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
        DomainEventDispatcher.Raise(personExpiredEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personExpiredEvent.PersonId);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
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
        DomainEventDispatcher.Raise(moveEvent);
        await removedEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(new PersonCreatedEvent(additionalPersonId, new Location(2, 2)));
        await DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        var moveEvent = new PersonLocationChangedEvent(additionalPersonId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEventDispatcher.Raise(moveEvent);
        await removedEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
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
        DomainEventDispatcher.Raise(new PersonCreatedEvent(newPersonId, new Location(2, 2)));

        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;
        personAddedToHazardZoneEvent.Should().NotBeNull();
        personAddedToHazardZoneEvent.PersonId.Should().Be(newPersonId);

        var personExpiredEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonExpiredEvent>();

        // Act
        DomainEventDispatcher.Raise(new PersonExpiredEvent(newPersonId));
        await personExpiredEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        DomainEventDispatcher.Raise(personExpiredEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personExpiredEvent.PersonId);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        DomainEventDispatcher.Raise(moveEvent);
        await removedEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
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
        DomainEventDispatcher.Raise(new PersonCreatedEvent(additionalPersonId, new Location(2, 2)));
        await DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEvent>();

        var moveEvent = new PersonLocationChangedEvent(additionalPersonId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEvent>();

        // Act
        DomainEventDispatcher.Raise(moveEvent);
        await removedEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
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
        hazardZone.AllowedNumberOfPersons.Should().Be(1);
    }

    public void Dispose()
    {
        DomainEventDispatcher.Dispose();
    }
}
