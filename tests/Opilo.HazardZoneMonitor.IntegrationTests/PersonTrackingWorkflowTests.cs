using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;
using Opilo.HazardZoneMonitor.Features.PersonTracking.Events;
using Opilo.HazardZoneMonitor.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Shared.Events;
using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.IntegrationTests;

/// <summary>
/// Integration tests for the complete person tracking workflow across all features.
/// Tests the interaction between PersonTracking, FloorManagement, and HazardZoneManagement.
/// </summary>
public sealed class PersonTrackingWorkflowTests : IDisposable
{
    private readonly Floor _floor;
    private readonly HazardZone _hazardZone;

    public PersonTrackingWorkflowTests()
    {
        Outline testOutline =
            new(new ReadOnlyCollection<Location>(new[]
        {
            new Location(0, 0),
            new Location(10, 0),
            new Location(10, 10),
            new Location(0, 10)
        }));

        _floor = new Floor("Test Floor", testOutline);
        _hazardZone = new HazardZone("Test Zone", testOutline, TimeSpan.FromMilliseconds(50));
        _hazardZone.ManuallyActivate();
    }

    [Fact]
    public async Task PersonTrackingWorkflow_ShouldTriggerFloorAndHazardZoneEvents_WhenPersonEntersFloor()
    {
        var personId = Guid.NewGuid();
        var location = new Location(5, 5); // Inside the test outline
        var personLocationUpdate = new PersonLocationUpdate(personId, location);

        var personAddedToFloorTask = WaitForEvent<PersonAddedToFloorEvent>(TimeSpan.FromSeconds(1));
        var personAddedToHazardZoneTask = WaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromSeconds(1));

        _floor.TryAddPersonLocationUpdate(personLocationUpdate);

        var floorEvent = await personAddedToFloorTask;
        var zoneEvent = await personAddedToHazardZoneTask;

        floorEvent.Should().NotBeNull();
        floorEvent.FloorName.Should().Be("Test Floor");
        floorEvent.PersonId.Should().Be(personId);
        floorEvent.Location.Should().Be(location);

        zoneEvent.Should().NotBeNull();
        zoneEvent.HazardZoneName.Should().Be("Test Zone");
        zoneEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public async Task PersonTrackingWorkflow_ShouldTriggerRemovalEvents_WhenPersonLeavesFloor()
    {
        var personId = Guid.NewGuid();
        var insideLocation = new Location(5, 5);
        var outsideLocation = new Location(50, 50); // Outside the test outline

        _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, insideLocation));
        await Task.Delay(50); // Let events propagate

        var personRemovedFromFloorTask = WaitForEvent<PersonRemovedFromFloorEvent>(TimeSpan.FromSeconds(1));
        var personRemovedFromZoneTask = WaitForEvent<PersonRemovedFromHazardZoneEvent>(TimeSpan.FromSeconds(1));

        _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, outsideLocation));

        var floorEvent = await personRemovedFromFloorTask;
        var zoneEvent = await personRemovedFromZoneTask;

        floorEvent.Should().NotBeNull();
        floorEvent.FloorName.Should().Be("Test Floor");
        floorEvent.PersonId.Should().Be(personId);

        zoneEvent.Should().NotBeNull();
        zoneEvent.HazardZoneName.Should().Be("Test Zone");
        zoneEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public async Task PersonTrackingWorkflow_ShouldRemoveFromFloorAndHazardZone_WhenPersonExpires()
    {
        var personId = Guid.NewGuid();
        var location = new Location(5, 5);

        _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, location));
        await Task.Delay(50); // Let initial events propagate

        var personRemovedFromFloorTask = WaitForEvent<PersonRemovedFromFloorEvent>(TimeSpan.FromSeconds(1));
        var personRemovedFromZoneTask = WaitForEvent<PersonRemovedFromHazardZoneEvent>(TimeSpan.FromSeconds(1));
        var personExpiredTask = WaitForEvent<PersonExpiredEvent>(TimeSpan.FromSeconds(1));

        await Task.Delay(250);

        var expiredEvent = await personExpiredTask;
        var floorEvent = await personRemovedFromFloorTask;
        var zoneEvent = await personRemovedFromZoneTask;

        expiredEvent.Should().NotBeNull();
        expiredEvent.PersonId.Should().Be(personId);

        floorEvent.Should().NotBeNull();
        floorEvent.PersonId.Should().Be(personId);

        zoneEvent.Should().NotBeNull();
        zoneEvent.PersonId.Should().Be(personId);
    }

    [Fact]
    public async Task PersonTrackingWorkflow_ShouldTriggerAlarm_WhenMultiplePersonsEnterHazardZoneAndLimitIsExceeded()
    {
        _hazardZone.SetAllowedNumberOfPersons(1); // Only allow 1 person

        var person1Id = Guid.NewGuid();
        var person2Id = Guid.NewGuid();
        var location1 = new Location(2, 2);
        var location2 = new Location(7, 7);

        _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(person1Id, location1));
        await Task.Delay(50);

        var initialState = _hazardZone.AlarmState;

        _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(person2Id, location2));
        await WaitUntilAsync(() => _hazardZone.AlarmState != AlarmState.None, TimeSpan.FromSeconds(2));

        initialState.Should().Be(AlarmState.None); // Was Active (no alarm)
        _hazardZone.AlarmState.Should().NotBe(AlarmState.None); // Now in alarm state
    }

    [Fact]
    public void PersonTrackingWorkflow_ShouldUpdateLocationWithoutRemoval_WhenPersonMovesWithinFloor()
    {
        var personId = Guid.NewGuid();
        var location1 = new Location(2, 2);
        var location2 = new Location(8, 8);

        var result1 = _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, location1));
        var result2 = _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, location2));

        result1.Should().BeTrue(); // First location added successfully
        result2.Should().BeTrue(); // Second location updated successfully
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);

        while (!condition())
        {
            await Task.Delay(10, cts.Token);
        }
    }

    private async Task<T?> WaitForEvent<T>(TimeSpan timeout) where T : IDomainEvent
    {
        var tcs = new TaskCompletionSource<T?>();

        using var cts = new CancellationTokenSource(timeout);

        void Handler(T evt)
        {
            tcs.TrySetResult(evt);
        }

        DomainEventDispatcher.Register<T>(Handler);

        cts.Token.Register(() =>
        {
            tcs.TrySetResult(default);
        });

        return await tcs.Task;
    }

    public void Dispose()
    {
        _floor.Dispose();
        _hazardZone.Dispose();
        DomainEventDispatcher.Dispose();
    }
}

