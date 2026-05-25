using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

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
        var clock = new FakeClock(DateTime.UnixEpoch);
        var hazardZoneOptions = HazardZoneOptionsBuilder.Create()
            .WithHazardZone(hazardZoneName.Value, zone => zone
                .WithRectangleOutline(0, 0, 10, 10)
                .WithActivationDuration(TimeSpan.FromSeconds(1))
                .WithPreAlarmDuration(TimeSpan.FromSeconds(1)))
            .Build();

        await using var host = factory.CreateHost()
            .WithHazardZoneConfiguration(hazardZoneOptions)
            .WithFakeTime(clock)
            .Start();
        var client = host.CreateClient();
        using var activateContent = new StringContent(string.Empty);
        using var deactivateContent = new StringContent(string.Empty);

        var hazardZone = await GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);

        var activateResponse = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/existing-hazardzone/activate", UriKind.Relative),
            activateContent,
            TestContext.Current.CancellationToken);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        clock.AdvanceBy(TimeSpan.FromSeconds(1));
        hazardZone = await GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/existing-hazardzone/deactivate", UriKind.Relative),
            deactivateContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        hazardZone = await GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);
    }

    private static async Task<HazardZoneInfo> GetCurrentHazardZone(HttpClient client, HazardZoneName hazardZoneName)
    {
        var currentHazardZones = await client.GetFromJsonAsync<GetHazardZonesResponse>(
            new Uri("/api/v1/hazard-zones", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken);

        currentHazardZones.Should().NotBeNull();

        return currentHazardZones.HazardZones.Single(hazardZone => hazardZone.Name == hazardZoneName);
    }
}
