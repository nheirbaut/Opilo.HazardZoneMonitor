using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class ActivateHazardZoneSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task ActivateHazardZone_ShouldReturn404NotFound_WhenHazardZoneDoesNotExist()
    {
        // Arrange
        var client = factory.CreateClient();
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
        var hazardZoneOptions = new HazardZoneOptions
        {
            HazardZones =
            [
                new HazardZoneConfiguration(
                    HazardZoneName.From("existing-hazardzone"),
                    [new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 10), new Coordinate(0, 10)],
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1))
            ]
        };

        await using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(hazardZoneOptions.ToConfigurationDictionary());
            });
        });

        var client = customFactory.CreateClient();
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
        var timerFactory = new FakeTimerFactory(clock);
        var hazardZoneOptions = new HazardZoneOptions
        {
            HazardZones =
            [
                new HazardZoneConfiguration(
                    hazardZoneName,
                    [new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 10), new Coordinate(0, 10)],
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1))
            ]
        };

        await using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(hazardZoneOptions.ToConfigurationDictionary());
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IClock>();
                services.RemoveAll<ITimerFactory>();
                services.AddSingleton<IClock>(clock);
                services.AddSingleton<ITimerFactory>(timerFactory);
            });
        });

        var client = customFactory.CreateClient();
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
