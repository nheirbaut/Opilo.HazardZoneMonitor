using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.Floors;

public sealed class GetFloorsSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetFloors_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/v1/floors", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFloors_ShouldSendResponseWithoutFloors_WhenNoFloorsAreRegistered()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetFloorsResponse>(
            new Uri("/api/v1/floors", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Floors.Should().NotBeNull();
        response.Floors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFloors_ShouldSendResponseWithFloors_WhenFloorsAreRegistered()
    {
        // Arrange
        List<FloorConfiguration> expectedFloors =
        [
            new(FloorName.From("First Floor"),
                new List<Coordinate>
                {
                    new(0, 0),
                    new(10, 0),
                    new(10, 10),
                    new(0, 10)
                }),
            new(FloorName.From("Second Floor"),
                new List<Coordinate>
                {
                    new(0, 0),
                    new(15, 0),
                    new(15, 15),
                    new(0, 15)
                })
        ];
        var floorOptions = new FloorOptions { Floors = expectedFloors };

        await using var host = factory.CreateHost()
            .WithFloorConfiguration(floorOptions)
            .Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetFloorsResponse>(
            new Uri("/api/v1/floors", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Floors.Should().NotBeNullOrEmpty();
        response.Floors.Should().BeEquivalentTo(expectedFloors);
    }

    [Fact]
    public async Task GetFloors_ShouldReturnFloorsWithHazardZones_WhenFloorsHaveHazardZonesConfigured()
    {
        // Arrange
        List<HazardZoneConfiguration> expectedHazardZones =
        [
            new(HazardZoneName.From("Reactor Room"),
            [
                new(2, 2),
                new(8, 2),
                new(8, 8),
                new(2, 8),
            ],
            TimeSpan.Zero,
            TimeSpan.Zero),
        ];

        List<FloorConfiguration> expectedFloors =
        [
            new(FloorName.From("Ground Floor"),
                new List<Coordinate>
                {
                    new(0, 0),
                    new(20, 0),
                    new(20, 20),
                    new(0, 20),
                })
            {
                HazardZones = expectedHazardZones,
            },
        ];

        var floorOptions = new FloorOptions { Floors = expectedFloors };

        await using var host = factory.CreateHost()
            .WithFloorConfiguration(floorOptions)
            .Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetFloorsResponse>(
            new Uri("/api/v1/floors", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken
        );

        // Assert
        response.Should().NotBeNull();
        response.Floors.Should().ContainSingle();
        response.Floors[0].HazardZones.Should().NotBeNullOrEmpty();
        response.Floors[0].HazardZones.Should().BeEquivalentTo(expectedHazardZones);
    }
}
