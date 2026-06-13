using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
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
    private static readonly HazardZoneName s_hazardZoneName = HazardZoneName.From("Reactor Room");

    private static readonly HazardZoneOptions s_hazardZoneOptions = HazardZoneOptionsBuilder.Create()
        .WithHazardZone(s_hazardZoneName.Value, z => z
            .WithRectangleOutline(2, 2, 8, 8)
            .WithAllowedNumberOfPersons(0)
            .WithPreAlarmDuration(TimeSpan.Zero))
        .Build();

    private static readonly FloorOptions s_floorOptionsSimple = FloorOptionsBuilder.Create()
        .WithFloor("Main Floor", f => f.WithRectangleOutline(0, 0, 10, 10))
        .Build();

    private static readonly FloorOptions s_floorOptionsWithHazardZone = FloorOptionsBuilder.Create()
        .WithFloor("Main Floor", f => f
            .WithRectangleOutline(0, 0, 10, 10)
            .WithHazardZone(s_hazardZoneName.Value, z => z
                .WithRectangleOutline(2, 2, 8, 8)
                .WithAllowedNumberOfPersons(0)
                .WithPreAlarmDuration(TimeSpan.Zero)))
        .Build();

    private static Command CreateDefaultRequest() =>
        new(PersonId.From(Guid.NewGuid()), new Coordinate(1, 1));

    private static async Task<HttpResponseMessage> RegisterPersonMovement(HttpClient client, Command request) =>
        await client.PostAsJsonAsync("/api/v1/person-movements", request, SerializationOptions.Default, TestContext.Current.CancellationToken);

    [Fact]
    public async Task RegisterPersonMovement_ShouldReturn201Created_WhenCalled()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();
        var request = CreateDefaultRequest();

        // Act
        var response = await RegisterPersonMovement(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldIncludeRegisteredAtTimestamp_WhenCalled()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();
        var request = CreateDefaultRequest();

        // Act
        var response = await RegisterPersonMovement(client, request);

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
        var request = CreateDefaultRequest();

        // Act
        var response = await RegisterPersonMovement(client, request);

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
        await using var host = factory.CreateHost()
            .WithFloorConfiguration(s_floorOptionsSimple)
            .WithHazardZoneConfiguration(s_hazardZoneOptions)
            .Start();
        var client = host.CreateClient();

        await HazardZoneApi.ActivateHazardZone(client, s_hazardZoneName);

        var request = new Command(PersonId.From(Guid.NewGuid()), new Coordinate(4, 4));

        // Act
        var response = await RegisterPersonMovement(client, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var hazardZone = await HazardZoneApi.GetCurrentHazardZone(client, s_hazardZoneName);
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldClearHazardZoneAlarm_WhenPersonExpires()
    {
        // Arrange
        var clock = new FakeClock();
        await using var host = factory.CreateHost()
            .WithFloorConfiguration(s_floorOptionsWithHazardZone)
            .WithFakeTime(clock)
            .Start();
        var client = host.CreateClient();

        await HazardZoneApi.ActivateHazardZone(client, s_hazardZoneName);

        var request = new Command(PersonId.From(Guid.NewGuid()), new Coordinate(4, 4));
        await RegisterPersonMovement(client, request);

        // Act
        clock.AdvanceBy(TimeSpan.FromMilliseconds(500));

        // Assert
        var hazardZone = await HazardZoneApi.GetCurrentHazardZone(client, s_hazardZoneName);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }

    [Fact]
    public async Task RegisterPersonMovement_ShouldClearHazardZoneAlarm_WhenPersonMovesOutsideFloor()
    {
        // Arrange
        var clock = new FakeClock();
        await using var host = factory.CreateHost()
            .WithFloorConfiguration(s_floorOptionsWithHazardZone)
            .WithFakeTime(clock)
            .Start();
        var client = host.CreateClient();

        await HazardZoneApi.ActivateHazardZone(client, s_hazardZoneName);

        var personId = PersonId.From(Guid.NewGuid());
        var insideRequest = new Command(personId, new Coordinate(4, 4));
        await RegisterPersonMovement(client, insideRequest);

        var hazardZone = await HazardZoneApi.GetCurrentHazardZone(client, s_hazardZoneName);
        hazardZone.AlarmState.Should().Be(AlarmState.Alarm);

        var outsideRequest = new Command(personId, new Coordinate(15, 15));

        // Act
        var response = await RegisterPersonMovement(client, outsideRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        hazardZone = await HazardZoneApi.GetCurrentHazardZone(client, s_hazardZoneName);
        hazardZone.AlarmState.Should().Be(AlarmState.None);
    }
}
