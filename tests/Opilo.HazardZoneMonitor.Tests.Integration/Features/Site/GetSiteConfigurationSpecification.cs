using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Site.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.Site;

public sealed class GetSiteConfigurationSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetSiteConfiguration_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            new Uri("/api/v1/site", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSiteConfiguration_ShouldReturnSiteConfiguration_WhenSiteIsRegistered()
    {
        // Arrange
        var expectedSite = new SiteConfiguration(SiteName.From("Reactor Facility Alpha").Value, []);

        var siteOptions = new SiteOptions { Name = SiteName.From(expectedSite.Name) };

        await using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(
                (_, config) =>
                {
                    config.AddInMemoryCollection(siteOptions.ToConfigurationDictionary());
                }
            );
        });

        var client = customFactory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetSiteResponse>(
            new Uri("/api/v1/site", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken
        );

        // Assert
        response.Should().NotBeNull();
        response.Site.Should().BeEquivalentTo(expectedSite);
    }

    [Fact]
    public async Task GetSiteConfiguration_ShouldReturnSiteWithFloors_WhenFloorsAreRegistered()
    {
        // Arrange
        List<FloorConfiguration> expectedFloors =
        [
            new(FloorName.From("Ground Floor"),
                new List<Coordinate>
                {
                    new(0, 0),
                    new(20, 0),
                    new(20, 20),
                    new(0, 20),
                }),
            new(FloorName.From("Upper Floor"),
                new List<Coordinate>
                {
                    new(0, 0),
                    new(15, 0),
                    new(15, 15),
                    new(0, 15),
                }),
        ];

        var siteOptions = new SiteOptions { Name = SiteName.From("Reactor Facility Alpha") };
        var floorOptions = new FloorOptions { Floors = expectedFloors };

        await using var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(
                (_, config) =>
                {
                    config.AddInMemoryCollection(siteOptions.ToConfigurationDictionary());
                    config.AddInMemoryCollection(floorOptions.ToConfigurationDictionary());
                }
            );
        });

        var client = customFactory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetSiteResponse>(
            new Uri("/api/v1/site", UriKind.Relative),
            SerializationOptions.Default,
            TestContext.Current.CancellationToken
        );

        // Assert
        response.Should().NotBeNull();
        response.Site.Name.Should().Be(siteOptions.Name.Value);
        response.Site.Floors.Should().NotBeNullOrEmpty();
        response.Site.Floors.Should().BeEquivalentTo(expectedFloors);
    }
}
