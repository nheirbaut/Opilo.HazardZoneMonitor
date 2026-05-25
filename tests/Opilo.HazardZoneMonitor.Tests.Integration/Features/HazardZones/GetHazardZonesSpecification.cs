using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class GetHazardZonesSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetHazardZones_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/v1/hazard-zones", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHazardZones_ShouldSendResponseWithoutHazardZones_WhenNoHazardZonesAreRegistered()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetHazardZonesResponse>(
            new Uri("/api/v1/hazard-zones", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.HazardZones.Should().NotBeNull();
        response.HazardZones.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHazardZones_ShouldSendResponseWithHazardZones_WhenHazardZonesAreRegistered()
    {
        // Arrange
        var hazardZoneOptions = HazardZoneOptionsBuilder.Create()
            .WithHazardZone("Reactor Room", zone => zone.WithRectangleOutline(0, 0, 10, 10))
            .WithHazardZone("Chemical Storage", zone => zone.WithRectangleOutline(20, 20, 35, 35))
            .Build();

        await using var host = factory.CreateHost()
            .WithHazardZoneConfiguration(hazardZoneOptions)
            .Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetHazardZonesResponse>(
            new Uri("/api/v1/hazard-zones", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.HazardZones.Should().NotBeNullOrEmpty();
        response.HazardZones.Should().BeEquivalentTo(hazardZoneOptions.HazardZones);
    }
}
