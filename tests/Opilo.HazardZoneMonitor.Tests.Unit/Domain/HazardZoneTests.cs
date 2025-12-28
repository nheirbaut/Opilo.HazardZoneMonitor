// ReSharper disable AccessToDisposedClosure

using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Unit.TestUtilities;
using Opilo.HazardZoneMonitor.Tests.Unit.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Tests.Unit.Domain;

public sealed class HazardZoneTests : IDisposable
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        var act = () => new HazardZone(null!, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act & Assert
        var act = () => new HazardZone(invalidName, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOutlineIsNull()
    {
        // Act & Assert
        var act = () => new HazardZone(HazardZoneBuilder.DefaultName, null!, TimeSpan.Zero);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldCreateInactiveInstance_WhenConstructorParametersAreValid()
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
    public void Constructor_ShouldThrowArgumentException_WhenPreAlarmDurationIsNegative()
    {
        // Act & Assert
        var act = () => new HazardZone(HazardZoneBuilder.DefaultName, HazardZoneBuilder.DefaultOutline,
            TimeSpan.FromMilliseconds(-100));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OnPersonCreatedEvent_ShouldRaisePersonAddedEvent_WhenPersonIsCreatedInZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        // Assert
        var personAddedToHazardZoneEvent = personAddedEvents.Single();
        personAddedToHazardZoneEvent.PersonId.Should().Be(newPersonId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public void OnPersonCreatedEvent_ShouldNotRaisePersonAddedEvent_WhenPersonIsCreatedOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var newPersonId = Guid.NewGuid();
        var locationOutsideZone = hazardZone.GetLocationOutside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonCreated(newPersonId, locationOutsideZone);

        // Assert
        personAddedEvents.Should().BeEmpty();
    }

    [Fact]
    public void OnPersonExpiredEvent_ShouldRaisePersonRemovedEvent_WhenPersonExpiresInZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);
        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        // Act
        hazardZone.HandlePersonExpired(newPersonId);

        // Assert
        var personRemovedFromHazardZoneEvent = personRemovedEvents.Single();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(newPersonId);
        personRemovedFromHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public void OnPersonExpiredEvent_ShouldNotRaisePersonRemovedEvent_WhenPersonExpiresOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var newPersonId = Guid.NewGuid();
        var locationOutsideZone = hazardZone.GetLocationOutside();
        hazardZone.HandlePersonCreated(newPersonId, locationOutsideZone);

        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        // Act
        hazardZone.HandlePersonExpired(newPersonId);

        // Assert
        personRemovedEvents.Should().BeEmpty();
    }

    [Fact]
    public void OnPersonLocationChangedEvent_ShouldRaisePersonAddedEvent_WhenUnknownPersonMovesIntoZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personLocationChangedEvent = PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.Handle(personLocationChangedEvent);

        // Assert
        var personAddedToHazardZoneEvent = personAddedEvents.Single();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personLocationChangedEvent.PersonId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public void OnPersonLocationChangedEvent_ShouldNotRaisePersonAddedEvent_WhenUnknownPersonMovesOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedOutsideHazardZone(hazardZone);
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.Handle(personLocationChangedEvent);

        // Assert
        personAddedEvents.Should().BeEmpty();
    }

    [Fact]
    public void OnPersonLocationChangedEvent_ShouldRaisePersonRemovedEvent_WhenKnownPersonMovesOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var initialPersonLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);
        hazardZone.Handle(initialPersonLocationChangedEvent);

        var newPersonLocationChangedEvent = PersonHelper.CreatePersonLocationChangedEventLocatedOutsideHazardZone(
            hazardZone,
            initialPersonLocationChangedEvent.PersonId);

        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        // Act
        hazardZone.Handle(newPersonLocationChangedEvent);

        // Assert
        var personRemovedFromHazardZoneEvent = personRemovedEvents.Single();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(initialPersonLocationChangedEvent.PersonId);
        personRemovedFromHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public void OnPersonLocationChangedEvent_ShouldNotRaiseEvents_WhenKnownPersonMovesWithinZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var initialPersonLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);

        hazardZone.Handle(initialPersonLocationChangedEvent);

        var newLocation = new Location(3, 3);
        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.Handle(new PersonLocationChangedEventArgs(initialPersonLocationChangedEvent.PersonId, newLocation));

        // Assert
        personRemovedEvents.Should().BeEmpty();
        personAddedEvents.Should().BeEmpty();
    }

    [Fact]
    public void ActivateFromExternalSource_ShouldThrowArgumentNullException_WhenSourceIdIsNull()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        var act = () => hazardZone.ActivateFromExternalSource(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void ActivateFromExternalSource_ShouldThrowArgumentException_WhenSourceIdIsInvalid(string invalidName)
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        var act = () => hazardZone.ActivateFromExternalSource(invalidName);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeactivateFromExternalSource_ShouldThrowArgumentNullException_WhenSourceIdIsNull()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act & Assert
        var act = () => hazardZone.DeactivateFromExternalSource(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void DeactivateFromExternalSource_ShouldThrowArgumentException_WhenSourceIdIsInvalid(string invalidName)
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
    public void ManuallyActivate_ShouldTransitionToActive_WhenInInactiveState()
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
    public void ActivateFromExternalSource_ShouldTransitionToActive_WhenInInactiveStateWithUnknownSource()
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
    public void ActivateFromExternalSource_ShouldNotTransition_WhenInInactiveStateWithKnownSource()
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
    public void ManuallyDeactivate_ShouldRemainInactive_WhenInInactiveState()
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
    public void OnPersonCreatedEvent_ShouldRemainActive_WhenInActiveStateUnderThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithAllowedNumberOfPersons(1)
            .WithState(HazardZoneTestState.Active)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        // Assert
        personAddedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_ShouldRemainActive_WhenInActiveStateAboveThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(3)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);
        personAddedEvents.Should().HaveCount(1);

        // Act
        hazardZone.SetAllowedNumberOfPersons(2);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void OnPersonCreatedEvent_ShouldTransitionToPreAlarm_WhenInActiveStateOverThresholdWithPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        // Assert
        personAddedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_ShouldTransitionToPreAlarm_WhenInActiveStateBelowThresholdWithPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);
        personAddedEvents.Should().HaveCount(1);

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public void OnPersonCreatedEvent_ShouldTransitionToAlarm_WhenInActiveStateOverThresholdWithZeroPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithPreAlarmDuration(TimeSpan.Zero)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        // Assert
        personAddedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void
        SetAllowedNumberOfPersons_ShouldTransitionToAlarm_WhenInActiveStateBelowThresholdWithZeroPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .WithPreAlarmDuration(TimeSpan.Zero)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);
        personAddedEvents.Should().HaveCount(1);

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void ManuallyDeactivate_ShouldTransitionToInactive_WhenInActiveState()
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
    public void DeactivateFromExternalSource_ShouldTransitionToInactive_WhenInActiveStateWithKnownSource()
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
    public void DeactivateFromExternalSource_ShouldRemainActive_WhenInActiveStateWithUnknownSource()
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
    public void ActivateFromExternalSource_ShouldAddSourceAndRemainActive_WhenInActiveStateWithUnknownSource()
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
    public void ActivateFromExternalSource_ShouldRemainActiveWithoutAddingSource_WhenInActiveStateWithKnownSource()
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
    public void
        OnPersonLocationChangedEvent_ShouldRemainActive_WhenPersonMovesOutsideInActiveStateUnderThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .Build();
        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var addedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => addedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);
        addedEvents.Should().HaveCount(1);

        var locationOutsideZone = hazardZone.GetLocationOutside();
        var moveEvent = new PersonLocationChangedEventArgs(newPersonId, locationOutsideZone);
        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.Handle(moveEvent);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    //------------------------------------------------------------------------------
    // PreAlarm (IsActive=true, AlarmState=PreAlarm)
    //------------------------------------------------------------------------------

    [Fact]
    public void OnPersonExpiredEvent_ShouldRemainInPreAlarm_WhenInPreAlarmStateOverThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var secondPersonAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => secondPersonAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        secondPersonAddedEvents.Should().HaveCount(1);

        var firstPersonToExpireId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var firstPersonRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => firstPersonRemovedEvents.Add(e);

        // Act
        hazardZone.HandlePersonExpired(firstPersonToExpireId);

        // Assert
        var personRemovedFromHazardZoneEvent = firstPersonRemovedEvents.Single();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(firstPersonToExpireId);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_ShouldRemainInPreAlarm_WhenInPreAlarmStateBelowThreshold()
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
    public void DeactivateFromExternalSource_ShouldNotDeactivate_WhenInPreAlarmStateWithUnknownSource()
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
    public void OnPersonExpiredEvent_ShouldTransitionToActive_WhenInPreAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var personId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        // Act
        hazardZone.HandlePersonExpired(personId);

        // Assert
        var personRemovedFromHazardZoneEvent = personRemovedEvents.Single();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personId);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void
        SetAllowedNumberOfPersons_ShouldTransitionToActive_WhenAllowedNumberOfPersonsEqualsCountInPreAlarmState()
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
    public void PreAlarmTimer_ShouldTransitionToAlarm_WhenInPreAlarmStateAndTimerElapses()
    {
        var testPreAlarmDuration = TimeSpan.FromMilliseconds(10);

        var clock = new FakeClock(DateTime.UnixEpoch);
        var timerFactory = new FakeTimerFactory(clock);

        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithPreAlarmDuration(testPreAlarmDuration)
            .WithTime(clock, timerFactory)
            .Build();

        // Act
        clock.AdvanceBy(testPreAlarmDuration);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void ManuallyDeactivate_ShouldTransitionToInactive_WhenInPreAlarmState()
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
    public void DeactivateFromExternalSource_ShouldTransitionToInactive_WhenInPreAlarmStateWithKnownSource()
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
    public void OnPersonCreatedEvent_ShouldRemainInPreAlarm_WhenInPreAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(1)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        // Assert
        personAddedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public void
        OnPersonLocationChangedEvent_ShouldTransitionToActive_WhenPersonMovesOutsideInPreAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(1);
        using var hazardZone = hazardZoneBuilder.Build();
        var newPersonId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var moveEvent = new PersonLocationChangedEventArgs(newPersonId, new Location(20, 20));
        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.Handle(moveEvent);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void
        OnPersonLocationChangedEvent_ShouldRemainInPreAlarm_WhenPersonMovesOutsideInPreAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(0)
            .Build();
        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var additionalPersonAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => additionalPersonAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);
        additionalPersonAddedEvents.Should().HaveCount(1);

        var moveEvent = new PersonLocationChangedEventArgs(newPersonId, new Location(20, 20));
        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.Handle(moveEvent);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public void ManuallyActivate_ShouldRemainInPreAlarm_WhenInPreAlarmState()
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
    public void OnPersonExpiredEvent_ShouldRemainInAlarm_WhenInAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        var personAddedToHazardZoneEvent = personAddedEvents.Single();
        personAddedToHazardZoneEvent.PersonId.Should().Be(newPersonId);

        // Act
        hazardZone.HandlePersonExpired(newPersonId);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_ShouldRemainInAlarm_WhenInAlarmStateBelowThreshold()
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
    public void DeactivateFromExternalSource_ShouldNotDeactivate_WhenInAlarmStateWithUnknownSource()
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
    public void OnPersonExpiredEvent_ShouldTransitionToActive_WhenInAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var personId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        // Act
        hazardZone.HandlePersonExpired(personId);

        // Assert
        var personRemovedFromHazardZoneEvent = personRemovedEvents.Single();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personId);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_ShouldTransitionToActive_WhenAllowedNumberOfPersonsEqualsCountInAlarmState()
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
    public void ManuallyDeactivate_ShouldTransitionToInactive_WhenInAlarmState()
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
    public void DeactivateFromExternalSource_ShouldTransitionToInactive_WhenInAlarmStateWithKnownSource()
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
    public void OnPersonCreatedEvent_ShouldRemainInAlarm_WhenInAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(1)
            .Build();
        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        // Assert
        personAddedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void
        OnPersonLocationChangedEvent_ShouldTransitionToActive_WhenPersonMovesOutsideInAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(1);
        using var hazardZone = hazardZoneBuilder.Build();
        var newPersonId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var moveEvent = new PersonLocationChangedEventArgs(newPersonId, new Location(20, 20));
        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.Handle(moveEvent);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void OnPersonLocationChangedEvent_ShouldRemainInAlarm_WhenPersonMovesOutsideInAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(0)
            .Build();
        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var additionalPersonAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => additionalPersonAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);
        additionalPersonAddedEvents.Should().HaveCount(1);

        var moveEvent = new PersonLocationChangedEventArgs(newPersonId, new Location(20, 20));
        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.Handle(moveEvent);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void ManuallyActivate_ShouldRemainInAlarm_WhenInAlarmState()
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
    public void SetAllowedNumberOfPersons_ShouldIgnoreChange_WhenAllowedNumberOfPersonsIsNegative()
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
    }
}
