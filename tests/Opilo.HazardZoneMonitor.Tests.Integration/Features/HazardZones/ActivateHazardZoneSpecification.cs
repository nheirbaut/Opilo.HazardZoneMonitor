using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class ActivateHazardZoneSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ActivateHazardZone_ShouldReturn404NotFound_WhenHazardZoneDoesNotExist()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();
        using var emptyContent = new StringContent(string.Empty);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/non-existing/activate", UriKind.Relative),
            emptyContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateHazardZone_ShouldReturn204NoContent_WhenHazardZoneExists()
    {
        // Arrange
        var hazardZoneOptions = HazardZoneOptionsBuilder.Create()
            .WithHazardZone("existing-hazardzone", zone => zone
                .WithRectangleOutline(0, 0, 10, 10)
                .WithActivationDuration(TimeSpan.FromSeconds(1))
                .WithPreAlarmDuration(TimeSpan.FromSeconds(1)))
            .Build();

        await using var host = factory.CreateHost()
            .WithHazardZoneConfiguration(hazardZoneOptions)
            .Start();
        var client = host.CreateClient();
        using var emptyContent = new StringContent(string.Empty);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/existing-hazardzone/activate", UriKind.Relative),
            emptyContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ActivateHazardZone_ShouldActivateHazardZone_WhenInactiveHazardZoneExistsAndActivationDelayElapsed()
    {
        // Arrange
        var hazardZoneName = HazardZoneName.From("existing-inactive-hazardzone");
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
        using var emptyContent = new StringContent(string.Empty);

        var hazardZone = await GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.ZoneState.Should().Be(ZoneState.Inactive);

        // Act
        var response = await client.PostAsync(
            new Uri("/api/v1/hazard-zones/existing-inactive-hazardzone/activate", UriKind.Relative),
            emptyContent,
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        hazardZone = await GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.ZoneState.Should().Be(ZoneState.Activating);

        clock.AdvanceBy(TimeSpan.FromSeconds(1));
        hazardZone = await GetCurrentHazardZone(client, hazardZoneName);
        hazardZone.ZoneState.Should().Be(ZoneState.Active);
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
