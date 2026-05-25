using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;
using Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.PersonTracking;

public sealed class RegisterPersonMovementsSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task RegisterPersonMovement_ShouldReturn201Created_WhenCalled()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();
        var personId = PersonId.From(Guid.NewGuid());
        var request = new Command(personId, new Coordinate(1, 1));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldIncludeRegisteredAtTimestamp_WhenCalled()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();
        var personId = PersonId.From(Guid.NewGuid());
        var request = new Command(personId, new Coordinate(1, 1));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

        // Assert
        var registeredPersonMovement = await response.Content.ReadFromJsonAsync<RegisteredPersonMovement>(SerializationOptions.Default, TestContext.Current.CancellationToken);
        registeredPersonMovement.Should().NotBeNull();
        registeredPersonMovement.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldReturnLocationHeader_WhenCalled()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();
        var personId = PersonId.From(Guid.NewGuid());
        var request = new Command(personId, new Coordinate(1, 1));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

        // Assert
        var registeredPersonMovement = await response.Content.ReadFromJsonAsync<RegisteredPersonMovement>(SerializationOptions.Default, TestContext.Current.CancellationToken);
        registeredPersonMovement.Should().NotBeNull();
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/v1/person-movements/{registeredPersonMovement.Id}");
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldUpdateHazardZoneAlarmState_WhenMovementIsInsideActiveHazardZone()
    {
        // Arrange
        var hazardZoneName = HazardZoneName.From("Reactor Room");
        var hazardZoneOptions = HazardZoneOptionsBuilder.Create()
            .WithHazardZone(hazardZoneName.Value, zone => zone
                .WithRectangleOutline(2, 2, 8, 8)
                .WithAllowedNumberOfPersons(0)
                .WithPreAlarmDuration(TimeSpan.Zero))
            .Build();

        await using var host = factory.CreateHost()
            .WithHazardZoneConfiguration(hazardZoneOptions)
            .Start();
        var client = host.CreateClient();

        await HazardZoneApi.ActivateHazardZone(client, hazardZoneName);

        var request = new Command(PersonId.From(Guid.NewGuid()), new Coordinate(4, 4));

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var hazardZone = await HazardZoneApi.GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldClearHazardZoneAlarm_WhenPersonExpires()
    {
        // Arrange
        var hazardZoneName = HazardZoneName.From("Reactor Room");
        var floorOptions = FloorOptionsBuilder.Create()
            .WithFloor("Main Floor", f => f
                .WithRectangleOutline(0, 0, 10, 10)
                .WithHazardZone(hazardZoneName.Value, z => z
                    .WithRectangleOutline(2, 2, 8, 8)
                    .WithAllowedNumberOfPersons(0)
                    .WithPreAlarmDuration(TimeSpan.Zero)))
            .Build();

        var hazardZoneOptions = HazardZoneOptionsBuilder.Create()
            .WithHazardZone(hazardZoneName.Value, z => z
                .WithRectangleOutline(2, 2, 8, 8)
                .WithAllowedNumberOfPersons(0)
                .WithPreAlarmDuration(TimeSpan.Zero))
            .Build();

        var clock = new FakeClock();
        await using var host = factory.CreateHost()
            .WithFloorConfiguration(floorOptions)
            .WithHazardZoneConfiguration(hazardZoneOptions)
            .WithFakeTime(clock)
            .Start();
        var client = host.CreateClient();

        await HazardZoneApi.ActivateHazardZone(client, hazardZoneName);

        var request = new Command(PersonId.From(Guid.NewGuid()), new Coordinate(4, 4));
        await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

        // Act
        clock.AdvanceBy(TimeSpan.FromMilliseconds(500));

        // Assert
        var hazardZone = await HazardZoneApi.GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }
}
