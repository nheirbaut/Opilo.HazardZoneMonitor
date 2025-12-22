using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Primitives;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class HazardZoneTests : IDisposable
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        var act = () => new HazardZone(null!, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero, new PersonEvents());
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act & Assert
        var act = () => new HazardZone(invalidName, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero, new PersonEvents());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOutlineIsNull()
    {
        // Act & Assert
        var act = () => new HazardZone(HazardZoneBuilder.DefaultName, null!, TimeSpan.Zero, new PersonEvents());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldCreateInactiveInstance_WhenConstructorParametersAreValid()
    {
        // Act
        using var hazardZone =
            new HazardZone(HazardZoneBuilder.DefaultName, HazardZoneBuilder.DefaultOutline, TimeSpan.Zero, new PersonEvents());

        // Assert
        hazardZone.Name.Should().Be(HazardZoneBuilder.DefaultName);
        hazardZone.Outline.Should().Be(HazardZoneBuilder.DefaultOutline);
        hazardZone.IsActive.Should().BeFalse();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void Constructor_ShouldAllowNegativePreAlarmDuration_WhenPreAlarmDurationIsNegative()
    {
        // Act
        using var hazardZone = new HazardZone(HazardZoneBuilder.DefaultName, HazardZoneBuilder.DefaultOutline,
            TimeSpan.FromMilliseconds(-100), new PersonEvents());

        // Assert
        hazardZone.PreAlarmDuration.Should().Be(TimeSpan.FromMilliseconds(-100));
    }

    [Fact]
    public async Task OnPersonCreatedEvent_ShouldRaisePersonAddedEvent_WhenPersonIsCreatedInZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);

        // Act
        hazardZone.Handle(personCreatedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().NotBeNull();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personCreatedEvent.PersonId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_ShouldNotRaisePersonAddedEvent_WhenPersonIsCreatedOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedOutsideHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions
                .RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                    h => hazardZone.PersonAddedToHazardZone += h,
                    h => hazardZone.PersonAddedToHazardZone -= h,
                    TimeSpan.FromMilliseconds(50));

        // Act
        hazardZone.Handle(personCreatedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().BeNull();
    }

    [Fact]
    public async Task OnPersonExpiredEvent_ShouldRaisePersonRemovedEvent_WhenPersonExpiresInZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        hazardZone.Handle(personCreatedEvent);
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
                h => hazardZone.PersonRemovedFromHazardZone += h,
                h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(new PersonExpiredEvent(personCreatedEvent.PersonId));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personCreatedEvent.PersonId);
        personRemovedFromHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public async Task OnPersonExpiredEvent_ShouldNotRaisePersonRemovedEvent_WhenPersonExpiresOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedOutsideHazardZone(hazardZone);
        hazardZone.Handle(personCreatedEvent);

        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
                h => hazardZone.PersonRemovedFromHazardZone += h,
                h => hazardZone.PersonRemovedFromHazardZone -= h,
                TimeSpan.FromMilliseconds(40));

        // Act
        hazardZone.Handle(new PersonExpiredEvent(personCreatedEvent.PersonId));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().BeNull();
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ShouldRaisePersonAddedEvent_WhenUnknownPersonMovesIntoZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personLocationChangedEvent = PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);

        // Act
        hazardZone.Handle(personLocationChangedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().NotBeNull();
        personAddedToHazardZoneEvent.PersonId.Should().Be(personLocationChangedEvent.PersonId);
        personAddedToHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ShouldNotRaisePersonAddedEvent_WhenUnknownPersonMovesOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var personLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedOutsideHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h,
                TimeSpan.FromMilliseconds(40));

        // Act
        hazardZone.Handle(personLocationChangedEvent);
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personAddedToHazardZoneEvent.Should().BeNull();
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ShouldRaisePersonRemovedEvent_WhenKnownPersonMovesOutsideZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var initialPersonLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);
        hazardZone.Handle(initialPersonLocationChangedEvent);

        var newPersonLocationChangedEvent = PersonHelper.CreatePersonLocationChangedEventLocatedOutsideHazardZone(
            hazardZone,
            initialPersonLocationChangedEvent.PersonId);

        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
                h => hazardZone.PersonRemovedFromHazardZone += h,
                h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(newPersonLocationChangedEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(initialPersonLocationChangedEvent.PersonId);
        personRemovedFromHazardZoneEvent.HazardZoneName.Should().Be(HazardZoneBuilder.DefaultName);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ShouldNotRaiseEvents_WhenKnownPersonMovesWithinZone()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.BuildSimple();

        var initialPersonLocationChangedEvent =
            PersonHelper.CreatePersonLocationChangedEventLocatedInHazardZone(hazardZone);

        hazardZone.Handle(initialPersonLocationChangedEvent);

        var newLocation = new Location(3, 3);
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
                h => hazardZone.PersonRemovedFromHazardZone += h,
                h => hazardZone.PersonRemovedFromHazardZone -= h,
                TimeSpan.FromMilliseconds(40));

        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h,
                TimeSpan.FromMilliseconds(40));

        // Act
        hazardZone.Handle(new PersonLocationChangedEvent(initialPersonLocationChangedEvent.PersonId, newLocation));
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;
        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().BeNull();
        personAddedToHazardZoneEvent.Should().BeNull();
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
    public async Task OnPersonCreatedEvent_ShouldRemainActive_WhenInActiveStateUnderThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithAllowedNumberOfPersons(1)
            .WithState(HazardZoneTestState.Active)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);

        // Act
        hazardZone.Handle(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public async Task SetAllowedNumberOfPersons_ShouldRemainActive_WhenInActiveStateAboveThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(3)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);
        hazardZone.Handle(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Act
        hazardZone.SetAllowedNumberOfPersons(2);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_ShouldTransitionToPreAlarm_WhenInActiveStateOverThresholdWithPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);

        // Act
        hazardZone.Handle(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public async Task SetAllowedNumberOfPersons_ShouldTransitionToPreAlarm_WhenInActiveStateBelowThresholdWithPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);
        hazardZone.Handle(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Act
        hazardZone.SetAllowedNumberOfPersons(0);

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public async Task OnPersonCreatedEvent_ShouldTransitionToAlarm_WhenInActiveStateOverThresholdWithZeroPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithPreAlarmDuration(TimeSpan.Zero)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);

        // Act
        hazardZone.Handle(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public async Task SetAllowedNumberOfPersons_ShouldTransitionToAlarm_WhenInActiveStateBelowThresholdWithZeroPreAlarm()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .WithPreAlarmDuration(TimeSpan.Zero)
            .Build();

        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);
        hazardZone.Handle(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

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
    public async Task OnPersonLocationChangedEvent_ShouldRemainActive_WhenPersonMovesOutsideInActiveStateUnderThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Active)
            .WithAllowedNumberOfPersons(1)
            .Build();
        var personId = Guid.NewGuid();
        var initialEvent = new PersonCreatedEvent(personId, new Location(2, 2));
            var addedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);
            hazardZone.Handle(initialEvent);
            await addedEventTask;

        var moveEvent = new PersonLocationChangedEvent(personId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
            h => hazardZone.PersonRemovedFromHazardZone += h,
            h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(moveEvent);
        await removedEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    //------------------------------------------------------------------------------
    // PreAlarm (IsActive=true, AlarmState=PreAlarm)
    //------------------------------------------------------------------------------

    [Fact]
    public async Task OnPersonExpiredEvent_ShouldRemainInPreAlarm_WhenInPreAlarmStateOverThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var secondPersonAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);
        hazardZone.Handle(new PersonCreatedEvent(Guid.NewGuid(), new Location(2, 2)));

        var secondPersonAddedToHazardZoneEvent = await secondPersonAddedToHazardZoneEventTask;
        secondPersonAddedToHazardZoneEvent.Should().NotBeNull();

        var firstPersonExpiredEvent = new PersonExpiredEvent(hazardZoneBuilder.IdsOfPersonsAdded.First());
        var firstPersonRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
                h => hazardZone.PersonRemovedFromHazardZone += h,
                h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(firstPersonExpiredEvent);
        var personRemovedFromHazardZoneEvent = await firstPersonRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(firstPersonExpiredEvent.PersonId);
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
    public async Task OnPersonExpiredEvent_ShouldTransitionToActive_WhenInPreAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var personExpiredEvent = new PersonExpiredEvent(hazardZoneBuilder.IdsOfPersonsAdded.First());
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
                h => hazardZone.PersonRemovedFromHazardZone += h,
                h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(personExpiredEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personExpiredEvent.PersonId);
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public void SetAllowedNumberOfPersons_ShouldTransitionToActive_WhenAllowedNumberOfPersonsEqualsCountInPreAlarmState()
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
    public async Task OnPersonCreatedEvent_ShouldRemainInPreAlarm_WhenInPreAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(1)
            .Build();
        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);

        // Act
        hazardZone.Handle(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.PreAlarm);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ShouldTransitionToActive_WhenPersonMovesOutsideInPreAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(1);
        using var hazardZone = hazardZoneBuilder.Build();
        var personId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var moveEvent = new PersonLocationChangedEvent(personId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
            h => hazardZone.PersonRemovedFromHazardZone += h,
            h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(moveEvent);
        await removedEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ShouldRemainInPreAlarm_WhenPersonMovesOutsideInPreAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.PreAlarm)
            .WithAllowedNumberOfPersons(0)
            .Build();
        var additionalPersonId = Guid.NewGuid();
        var additionalPersonAddedTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
            h => hazardZone.PersonAddedToHazardZone += h,
            h => hazardZone.PersonAddedToHazardZone -= h);
        hazardZone.Handle(new PersonCreatedEvent(additionalPersonId, new Location(2, 2)));
        await additionalPersonAddedTask;

        var moveEvent = new PersonLocationChangedEvent(additionalPersonId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
            h => hazardZone.PersonRemovedFromHazardZone += h,
            h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(moveEvent);
        await removedEventTask;

        // Assert
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
    public async Task OnPersonExpiredEvent_ShouldRemainInAlarm_WhenInAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .Build();

        var newPersonId = Guid.NewGuid();
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);
        hazardZone.Handle(new PersonCreatedEvent(newPersonId, new Location(2, 2)));

        var personAddedToHazardZoneEvent = await personAddedToHazardZoneEventTask;
        personAddedToHazardZoneEvent.Should().NotBeNull();
        personAddedToHazardZoneEvent.PersonId.Should().Be(newPersonId);

        // Act
        hazardZone.Handle(new PersonExpiredEvent(newPersonId));

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
    public async Task OnPersonExpiredEvent_ShouldTransitionToActive_WhenInAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm);

        using var hazardZone = hazardZoneBuilder.Build();

        var personExpiredEvent = new PersonExpiredEvent(hazardZoneBuilder.IdsOfPersonsAdded.First());
        var personRemovedFromHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
                h => hazardZone.PersonRemovedFromHazardZone += h,
                h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(personExpiredEvent);
        var personRemovedFromHazardZoneEvent = await personRemovedFromHazardZoneEventTask;

        // Assert
        personRemovedFromHazardZoneEvent.Should().NotBeNull();
        personRemovedFromHazardZoneEvent.PersonId.Should().Be(personExpiredEvent.PersonId);
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
    public async Task OnPersonCreatedEvent_ShouldRemainInAlarm_WhenInAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(1)
            .Build();
        var personCreatedEvent = PersonHelper.CreatePersonCreatedEventLocatedInHazardZone(hazardZone);
        var personAddedToHazardZoneEventTask =
            DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
                h => hazardZone.PersonAddedToHazardZone += h,
                h => hazardZone.PersonAddedToHazardZone -= h);

        // Act
        hazardZone.Handle(personCreatedEvent);
        await personAddedToHazardZoneEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ShouldTransitionToActive_WhenPersonMovesOutsideInAlarmStateUnderThreshold()
    {
        // Arrange
        var hazardZoneBuilder = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(1);
        using var hazardZone = hazardZoneBuilder.Build();
        var personId = hazardZoneBuilder.IdsOfPersonsAdded.First();
        var moveEvent = new PersonLocationChangedEvent(personId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
            h => hazardZone.PersonRemovedFromHazardZone += h,
            h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(moveEvent);
        await removedEventTask;

        // Assert
        hazardZone.IsActive.Should().BeTrue();
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public async Task OnPersonLocationChangedEvent_ShouldRemainInAlarm_WhenPersonMovesOutsideInAlarmStateOverThreshold()
    {
        // Arrange
        using var hazardZone = HazardZoneBuilder.Create()
            .WithState(HazardZoneTestState.Alarm)
            .WithAllowedNumberOfPersons(0)
            .Build();
        var additionalPersonId = Guid.NewGuid();
        var additionalPersonAddedTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonAddedToHazardZoneEventArgs>(
            h => hazardZone.PersonAddedToHazardZone += h,
            h => hazardZone.PersonAddedToHazardZone -= h);
        hazardZone.Handle(new PersonCreatedEvent(additionalPersonId, new Location(2, 2)));
        await additionalPersonAddedTask;

        var moveEvent = new PersonLocationChangedEvent(additionalPersonId, new Location(20, 20));
        var removedEventTask = DomainEventsExtensions.RegisterAndWaitForEvent<PersonRemovedFromHazardZoneEventArgs>(
            h => hazardZone.PersonRemovedFromHazardZone += h,
            h => hazardZone.PersonRemovedFromHazardZone -= h);

        // Act
        hazardZone.Handle(moveEvent);
        await removedEventTask;

        // Assert
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
