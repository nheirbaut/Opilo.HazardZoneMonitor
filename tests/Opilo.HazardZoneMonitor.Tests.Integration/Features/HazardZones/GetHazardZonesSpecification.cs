using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class GetHazardZonesSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetHazardZones_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();

        // Arrange
        var response = await client.GetAsync(new Uri("/api/v1/hazard-zones", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHazardZones_ShouldSendResponseWithoutHazardZones_WhenNoHazardZonesAreRegistered()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<Response>(new Uri("/api/v1/hazard-zones", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.HazardZones.Should().NotBeNull();
        response.HazardZones.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHazardZones_ShouldSendResponseWithHazardZones_WhenHazardZonesAreRegistered()
    {
        // Arrange
        List<HazardZoneConfiguration> expectedHazardZones =
        [
            new("Reactor Room",
                [
                    new(0, 0),
                    new(10, 0),
                    new(10, 10),
                    new(0, 10)
                ]),
            new("Chemical Storage",
                [
                    new(20, 20),
                    new(35, 20),
                    new(35, 35),
                    new(20, 35)
                ])
        ];
        var hazardZoneOptions = new HazardZoneOptions(expectedHazardZones);

        await using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(hazardZoneOptions.ToConfigurationDictionary());
            });
        });

        var client = customFactory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<Response>(new Uri("/api/v1/hazard-zones", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.HazardZones.Should().NotBeNullOrEmpty();
        response.HazardZones.Should().BeEquivalentTo(expectedHazardZones);
    }
}

#pragma warning disable MA0048
public sealed record HazardZoneOptions(IReadOnlyList<HazardZoneConfiguration> HazardZones);

public sealed record HazardZoneConfiguration(string Name, IReadOnlyList<HazardZonePointConfiguration> Outline);

public sealed record HazardZonePointConfiguration(double X, double Y);

public static class ConfigurationExtensions
{

    public static IDictionary<string, string?> ToConfigurationDictionary(this HazardZoneOptions hazardZoneOptions)
    {
        ArgumentNullException.ThrowIfNull(hazardZoneOptions);

        Dictionary<string, string?> dictionary = new(StringComparer.Ordinal);
        var hazardZones = hazardZoneOptions.HazardZones;

        for (var hazardZoneIndex = 0; hazardZoneIndex < hazardZones.Count; hazardZoneIndex++)
        {
            var hazardZone = hazardZones[hazardZoneIndex];
            var hazardZoneKey = $"{nameof(HazardZoneOptions)}:{nameof(HazardZoneOptions.HazardZones)}:{hazardZoneIndex}";

            dictionary[$"{hazardZoneKey}:{nameof(HazardZoneConfiguration.Name)}"] = hazardZone.Name;

            for (var pointIndex = 0; pointIndex < hazardZone.Outline.Count; pointIndex++)
            {
                var point = hazardZone.Outline[pointIndex];
                var pointKey = $"{hazardZoneKey}:{nameof(HazardZoneConfiguration.Outline)}:{pointIndex}";

                dictionary[$"{pointKey}:{nameof(HazardZonePointConfiguration.X)}"] = point.X.ToString(CultureInfo.InvariantCulture);
                dictionary[$"{pointKey}:{nameof(HazardZonePointConfiguration.Y)}"] = point.Y.ToString(CultureInfo.InvariantCulture);
            }
        }

        return dictionary;
    }

}
#pragma warning restore MA0048
