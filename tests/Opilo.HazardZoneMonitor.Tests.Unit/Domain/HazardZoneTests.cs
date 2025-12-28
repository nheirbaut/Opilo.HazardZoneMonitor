// ReSharper disable AccessToDisposedClosure

using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
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
    public void HandlePersonCreated_ShouldRaisePersonAddedEvent_WhenPersonIsCreatedInZone()
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
    public void HandlePersonCreated_ShouldNotRaisePersonAddedEvent_WhenPersonIsCreatedOutsideZone()
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
    public void HandlePersonExpired_ShouldRaisePersonRemovedEvent_WhenPersonExpiresInZone()
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
    public void HandlePersonExpired_ShouldNotRaisePersonRemovedEvent_WhenPersonExpiresOutsideZone()
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
    public void HandlePersonLocationChanged_ShouldRaisePersonAddedEvent_WhenUnknownPersonMovesIntoZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(personId, locationInsideZone);

        // Assert
        var personAddedToHazardZoneEvent = personAddedEvents.Single();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public void HandlePersonLocationChanged_ShouldNotRaisePersonAddedEvent_WhenUnknownPersonMovesOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personId = Guid.NewGuid();
        var locationOutsideZone = hazardZone.GetLocationOutside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(personId, locationOutsideZone);

        // Assert
        personAddedEvents.Should().BeEmpty();
    }

    [Fact]
    public void HandlePersonLocationChanged_ShouldRaisePersonRemovedEvent_WhenKnownPersonMovesOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var locationOutsideZone = hazardZone.GetLocationOutside();
        hazardZone.HandlePersonLocationChanged(personId, locationInsideZone);

        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(personId, locationOutsideZone);

        // Assert
        var personRemovedFromHazardZoneEvent = personRemovedEvents.Single();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personId);
        personRemovedFromHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public void HandlePersonLocationChanged_ShouldNotRaiseEvents_WhenKnownPersonMovesWithinZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        hazardZone.HandlePersonLocationChanged(personId, locationInsideZone);

        var newLocation = new Location(locationInsideZone.X + 1, locationInsideZone.Y + 1);
        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(personId, newLocation);

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
    // Inactive (ZoneState=Inactive, AlarmState=None)
    //------------------------------------------------------------------------------

    [Fact]
    public void ManuallyActivate_ShouldTransitionToActive_WhenInInactiveState()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void ManuallyActivate_ShouldTransitionToActivatingState_WhenActivationDurationIsGreaterThanZero()
    {
        // Arrange
        var activationDuration = TimeSpan.FromSeconds(3);
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(activationDuration)
            .Build();

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        hazardZone.ZoneState.Should().Be(ZoneState.Activating);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void ManuallyActivate_ShouldTransitionToActive_WhenActivationDurationIsZero()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(TimeSpan.Zero)
            .Build();

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void ActivateFromExternalSource_ShouldTransitionToActivatingState_WhenActivationDurationIsGreaterThanZero()
    {
        // Arrange
        var activationDuration = TimeSpan.FromSeconds(3);
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(activationDuration)
            .Build();

        // Act
        hazardZone.ActivateFromExternalSource("ext-src");

        // Assert
        hazardZone.ZoneState.Should().Be(ZoneState.Activating);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void ActivateFromExternalSource_ShouldTransitionToActive_WhenActivationDurationIsZero()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(TimeSpan.Zero)
            .Build();

        // Act
        hazardZone.ActivateFromExternalSource("ext-src");

        // Assert
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    //------------------------------------------------------------------------------
    // Activating (ZoneState=Activating, AlarmState=None)
    //------------------------------------------------------------------------------

    [Fact]
    public void ActivationTimer_ShouldTransitionToActive_WhenInActivatingStateAndTimerElapses()
    {
        // Arrange
        var testActivationDuration = TimeSpan.FromMilliseconds(10);
        var clock = new FakeClock(DateTime.UnixEpoch);
        var timerFactory = new FakeTimerFactory(clock);

        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(testActivationDuration)
            .WithTime(clock, timerFactory)
            .Build();

        hazardZone.ManuallyActivate();

        hazardZone.ZoneState.Should().Be(ZoneState.Activating);

        // Act
        clock.AdvanceBy(testActivationDuration);

        // Assert
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void ManuallyDeactivate_ShouldTransitionToInactive_WhenInActivatingState()
    {
        // Arrange
        var activationDuration = TimeSpan.FromSeconds(3);
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(activationDuration)
            .Build();

        hazardZone.ManuallyActivate();

        // Act
        hazardZone.ManuallyDeactivate();

        // Assert
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void DeactivateFromExternalSource_ShouldTransitionToInactive_WhenInActivatingStateWithKnownSource()
    {
        // Arrange
        var sourceId = "ext-src";
        var activationDuration = TimeSpan.FromSeconds(3);
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(activationDuration)
            .Build();

        hazardZone.ActivateFromExternalSource(sourceId);

        // Act
        hazardZone.DeactivateFromExternalSource(sourceId);

        // Assert
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void ManuallyActivate_ShouldRaiseHazardZoneActivationStartedEvent_WhenTransitioningToActivatingState()
    {
        // Arrange
        var activationDuration = TimeSpan.FromSeconds(3);
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(activationDuration)
            .Build();

        var activationStartedEvents = new List<HazardZoneActivationStartedEventArgs>();
        hazardZone.HazardZoneActivationStarted += (_, e) => activationStartedEvents.Add(e);

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        var activationStartedEvent = activationStartedEvents.Single();
        activationStartedEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public void ActivateFromExternalSource_ShouldRaiseHazardZoneActivationStartedEvent_WhenTransitioningToActivatingState()
    {
        // Arrange
        var activationDuration = TimeSpan.FromSeconds(3);
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(activationDuration)
            .Build();

        var activationStartedEvents = new List<HazardZoneActivationStartedEventArgs>();
        hazardZone.HazardZoneActivationStarted += (_, e) => activationStartedEvents.Add(e);

        // Act
        hazardZone.ActivateFromExternalSource("Source1");

        // Assert
        var activationStartedEvent = activationStartedEvents.Single();
        activationStartedEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public void ManuallyActivate_ShouldNotRaiseHazardZoneActivationStartedEvent_WhenActivationDurationIsZeroAndTransitionsDirectlyToActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(TimeSpan.Zero)
            .Build();

        var activationStartedEvents = new List<HazardZoneActivationStartedEventArgs>();
        hazardZone.HazardZoneActivationStarted += (_, e) => activationStartedEvents.Add(e);

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        activationStartedEvents.Should().BeEmpty();
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
    }

    [Fact]
    public void ManuallyActivate_ShouldRaiseHazardZoneActivatedEvent_WhenActivationDurationIsZeroAndTransitionsDirectlyToActive()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(TimeSpan.Zero)
            .Build();

        var activatedEvents = new List<HazardZoneActivatedEventArgs>();
        hazardZone.HazardZoneActivated += (_, e) => activatedEvents.Add(e);

        // Act
        hazardZone.ManuallyActivate();

        // Assert
        var activatedEvent = activatedEvents.Single();
        activatedEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
    }

    [Fact]
    public void HandlePersonCreated_ShouldTrackPersonAndRemainActivating_WhenPersonEntersWhileActivating()
    {
        // Arrange
        var activationDuration = TimeSpan.FromSeconds(3);
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(activationDuration)
            .WithAllowedNumberOfPersons(1)
            .Build();

        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var personAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => personAddedEvents.Add(e);

        hazardZone.ManuallyActivate();

        // Act
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);

        // Assert
        personAddedEvents.Should().HaveCount(1);
        hazardZone.ZoneState.Should().Be(ZoneState.Activating);
    }

    [Fact]
    public void HandlePersonExpired_ShouldRemovePersonAndRemainActivating_WhenPersonExpiresWhileActivating()
    {
        // Arrange
        var activationDuration = TimeSpan.FromSeconds(3);
        var personId = Guid.NewGuid();
        using var hazardZone = HazardZoneBuilder.Create()
            .WithActivationDuration(activationDuration)
            .WithAllowedNumberOfPersons(1)
            .Build();

        var locationInsideZone = hazardZone.GetLocationInside();
        hazardZone.HandlePersonCreated(personId, locationInsideZone);

        hazardZone.ManuallyActivate();
        var personRemovedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => personRemovedEvents.Add(e);

        // Act
        hazardZone.HandlePersonExpired(personId);

        // Assert
        personRemovedEvents.Should().HaveCount(1);
        hazardZone.ZoneState.Should().Be(ZoneState.Activating);
    }

    //------------------------------------------------------------------------------
    // Active (ZoneState=Active, AlarmState=None)
    //------------------------------------------------------------------------------

    [Fact]
    public void HandlePersonCreated_ShouldRemainActive_WhenInActiveStateUnderThreshold()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void HandlePersonCreated_ShouldTransitionToPreAlarm_WhenInActiveStateOverThresholdWithPreAlarm()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public void HandlePersonCreated_ShouldTransitionToAlarm_WhenInActiveStateOverThresholdWithZeroPreAlarm()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void
        HandlePersonLocationChanged_ShouldRemainActive_WhenPersonMovesOutsideInActiveStateUnderThreshold()
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
        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(newPersonId, locationOutsideZone);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    //------------------------------------------------------------------------------
    // PreAlarm (ZoneState=Active, AlarmState=PreAlarm)
    //------------------------------------------------------------------------------

    [Fact]
    public void HandlePersonExpired_ShouldRemainInPreAlarm_WhenInPreAlarmStateOverThreshold()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public void HandlePersonExpired_ShouldTransitionToActive_WhenInPreAlarmStateUnderThreshold()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void HandlePersonCreated_ShouldRemainInPreAlarm_WhenInPreAlarmStateOverThreshold()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public void
        HandlePersonLocationChanged_ShouldTransitionToActive_WhenPersonMovesOutsideInPreAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(1);
        using var hazardZone = hazardZoneBuilder.Build();
        var personId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var locationOutsideZone = hazardZone.GetLocationOutside();
        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(personId, locationOutsideZone);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void
        HandlePersonLocationChanged_ShouldRemainInPreAlarm_WhenPersonMovesOutsideInPreAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(0)
            .Build();
        var newPersonId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var locationOutsideZone = hazardZone.GetLocationOutside();
        var additionalPersonAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => additionalPersonAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(newPersonId, locationInsideZone);
        additionalPersonAddedEvents.Should().HaveCount(1);

        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(newPersonId, locationOutsideZone);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    //------------------------------------------------------------------------------
    // Alarm (ZoneState=Active, AlarmState=Alarm)
    //------------------------------------------------------------------------------

    [Fact]
    public void HandlePersonExpired_ShouldRemainInAlarm_WhenInAlarmStateOverThreshold()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void HandlePersonExpired_ShouldTransitionToActive_WhenInAlarmStateUnderThreshold()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void HandlePersonCreated_ShouldRemainInAlarm_WhenInAlarmStateOverThreshold()
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public void
        HandlePersonLocationChanged_ShouldTransitionToActive_WhenPersonMovesOutsideInAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(1);
        using var hazardZone = hazardZoneBuilder.Build();
        var personId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var locationOutsideZone = hazardZone.GetLocationOutside();
        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(personId, locationOutsideZone);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void HandlePersonLocationChanged_ShouldRemainInAlarm_WhenPersonMovesOutsideInAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(0)
            .Build();
        var personId = Guid.NewGuid();
        var locationInsideZone = hazardZone.GetLocationInside();
        var locationOutsideZone = hazardZone.GetLocationOutside();
        var additionalPersonAddedEvents = new List<PersonAddedToHazardZoneEventArgs>();
        hazardZone.PersonAddedToHazardZone += (_, e) => additionalPersonAddedEvents.Add(e);
        hazardZone.HandlePersonCreated(personId, locationInsideZone);
        additionalPersonAddedEvents.Should().HaveCount(1);

        var removedEvents = new List<PersonRemovedFromHazardZoneEventArgs>();
        hazardZone.PersonRemovedFromHazardZone += (_, e) => removedEvents.Add(e);

        // Act
        hazardZone.HandlePersonLocationChanged(personId, locationOutsideZone);

        // Assert
        removedEvents.Should().HaveCount(1);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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

