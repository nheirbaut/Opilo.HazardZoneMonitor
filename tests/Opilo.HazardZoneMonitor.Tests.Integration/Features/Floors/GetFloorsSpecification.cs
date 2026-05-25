using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;
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
        var floorOptions = FloorOptionsBuilder.Create()
            .WithFloor("First Floor", floor => floor.WithRectangleOutline(0, 0, 10, 10))
            .WithFloor("Second Floor", floor => floor.WithRectangleOutline(0, 0, 15, 15))
            .Build();

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
        response.Floors.Should().BeEquivalentTo(floorOptions.Floors);
    }

    [Fact]
    public async Task GetFloors_ShouldReturnFloorsWithHazardZones_WhenFloorsHaveHazardZonesConfigured()
    {
        // Arrange
        var floorOptions = FloorOptionsBuilder.Create()
            .WithFloor("Ground Floor", floor => floor
                .WithRectangleOutline(0, 0, 20, 20)
                .WithHazardZone("Reactor Room", zone => zone.WithRectangleOutline(2, 2, 8, 8)))
            .Build();

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
        response.Floors[0].HazardZones.Should().BeEquivalentTo(floorOptions.Floors[0].HazardZones);
    }
}
