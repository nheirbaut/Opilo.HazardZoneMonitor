using System.Net;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class DeactivateHazardZoneSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task DeactivateHazardZone_ShouldReturn404NotFound_WhenHazardZoneDoesNotExist()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();
        using var emptyContent = new StringContent(string.Empty);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/non-existing/deactivate", UriKind.Relative),
            emptyContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateHazardZone_ShouldReturn204NoContent_WhenHazardZoneExists()
    {
        // Arrange
        var hazardZoneOptions = HazardZoneOptionsBuilder.Create()
            .WithHazardZone("existing-hazardzone", zone => zone
                .WithRectangleOutline(0, 0, 10, 10))
            .Build();

        await using var host = factory.CreateHost()
            .WithHazardZoneConfiguration(hazardZoneOptions)
            .Start();
        var client = host.CreateClient();
        using var emptyContent = new StringContent(string.Empty);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/existing-hazardzone/deactivate", UriKind.Relative),
            emptyContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeactivateHazardZone_ShouldDeactivateHazardZone_WhenHazardZoneExists()
    {
        // Arrange
        var hazardZoneName = HazardZoneName.From("existing-hazardzone");
        var hazardZoneOptions = HazardZoneOptionsBuilder.Create()
            .WithHazardZone(hazardZoneName.Value, zone => zone
                .WithRectangleOutline(0, 0, 10, 10))
            .Build();

        await using var host = factory.CreateHost()
            .WithHazardZoneConfiguration(hazardZoneOptions)
            .Start();
        var client = host.CreateClient();
        using var deactivateContent = new StringContent(string.Empty);

        await HazardZoneApi.ActivateHazardZone(client, hazardZoneName);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/existing-hazardzone/deactivate", UriKind.Relative),
            deactivateContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var hazardZone = await HazardZoneApi.GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
    }
}
