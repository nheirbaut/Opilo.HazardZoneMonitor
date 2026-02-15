using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
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

        // Act
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
        var hazardZoneOptions = new HazardZoneOptions { HazardZones = expectedHazardZones };

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
