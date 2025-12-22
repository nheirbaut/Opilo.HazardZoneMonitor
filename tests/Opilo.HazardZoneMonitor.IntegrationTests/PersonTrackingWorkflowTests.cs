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
    public async Task PersonEntersFloor_ShouldTriggerFloorAndHazardZoneEvents()
    {
        var personId = Guid.NewGuid();
        var location = new Location(5, 5); // Inside the test outline
        var personLocationUpdate = new PersonLocationUpdate(personId, location);

        var personAddedToFloorTask = WaitForEvent<PersonAddedToFloorEvent>(TimeSpan.FromSeconds(1));
        var personAddedToHazardZoneTask = WaitForEvent<PersonAddedToHazardZoneEvent>(TimeSpan.FromSeconds(1));

        _floor.TryAddPersonLocationUpdate(personLocationUpdate);

        var floorEvent = await personAddedToFloorTask;
        var zoneEvent = await personAddedToHazardZoneTask;

        Assert.NotNull(floorEvent);
        Assert.Equal("Test Floor", floorEvent.FloorName);
        Assert.Equal(personId, floorEvent.PersonId);
        Assert.Equal(location, floorEvent.Location);

        Assert.NotNull(zoneEvent);
        Assert.Equal("Test Zone", zoneEvent.HazardZoneName);
        Assert.Equal(personId, zoneEvent.PersonId);
    }

    [Fact]
    public async Task PersonLeavesFloor_ShouldTriggerRemovalEvents()
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

        Assert.NotNull(floorEvent);
        Assert.Equal("Test Floor", floorEvent.FloorName);
        Assert.Equal(personId, floorEvent.PersonId);

        Assert.NotNull(zoneEvent);
        Assert.Equal("Test Zone", zoneEvent.HazardZoneName);
        Assert.Equal(personId, zoneEvent.PersonId);
    }

    [Fact]
    public async Task PersonExpires_ShouldRemoveFromFloorAndHazardZone()
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

        Assert.NotNull(expiredEvent);
        Assert.Equal(personId, expiredEvent.PersonId);

        Assert.NotNull(floorEvent);
        Assert.Equal(personId, floorEvent.PersonId);

        Assert.NotNull(zoneEvent);
        Assert.Equal(personId, zoneEvent.PersonId);
    }

    [Fact]
    public async Task MultiplePersonsEnterHazardZone_ShouldTriggerAlarmWhenLimitExceeded()
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
        await Task.Delay(100); // Wait for pre-alarm timer to potentially elapse

        Assert.Equal(AlarmState.None, initialState); // Was Active (no alarm)
        Assert.True(_hazardZone.AlarmState != AlarmState.None); // Now in alarm state
    }

    [Fact]
    public void PersonMovesWithinFloor_ShouldUpdateLocationWithoutRemoval()
    {
        var personId = Guid.NewGuid();
        var location1 = new Location(2, 2);
        var location2 = new Location(8, 8);

        var result1 = _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, location1));
        var result2 = _floor.TryAddPersonLocationUpdate(new PersonLocationUpdate(personId, location2));

        Assert.True(result1); // First location added successfully
        Assert.True(result2); // Second location updated successfully
    }

    private Task<T?> WaitForEvent<T>(TimeSpan timeout) where T : IDomainEvent
    {
        var tcs = new TaskCompletionSource<T?>();

        #pragma warning disable CA2000 // Dispose objects before losing scope - disposed in callbacks
        var cts = new CancellationTokenSource(timeout);
        #pragma warning restore CA2000

        void Handler(T evt)
        {
            tcs.TrySetResult(evt);
            cts.Dispose();
        }

        DomainEventDispatcher.Register<T>(Handler);

        cts.Token.Register(() =>
        {
            tcs.TrySetResult(default);
            cts.Dispose();
        });

        return tcs.Task;
    }

    public void Dispose()
    {
        _floor.Dispose();
        _hazardZone.Dispose();
        DomainEventDispatcher.Dispose();
    }
}

