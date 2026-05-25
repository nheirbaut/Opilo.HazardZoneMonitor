using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.Site.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Site.GetSite;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.Site;

public sealed class GetSiteConfigurationSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetSiteConfiguration_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

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
        var expectedSite = new SiteConfiguration("Reactor Facility Alpha", []);

        var siteOptions = SiteOptionsBuilder.Create()
            .WithName(expectedSite.Name)
            .Build();

        await using var host = factory.CreateHost()
            .WithSiteConfiguration(siteOptions)
            .Start();
        var client = host.CreateClient();

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
        var siteOptions = SiteOptionsBuilder.Create()
            .WithName("Reactor Facility Alpha")
            .Build();
        var floorOptions = FloorOptionsBuilder.Create()
            .WithFloor("Ground Floor", floor => floor.WithRectangleOutline(0, 0, 20, 20))
            .WithFloor("Upper Floor", floor => floor.WithRectangleOutline(0, 0, 15, 15))
            .Build();

        await using var host = factory.CreateHost()
            .WithSiteConfiguration(siteOptions)
            .WithFloorConfiguration(floorOptions)
            .Start();
        var client = host.CreateClient();

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
        response.Site.Floors.Should().BeEquivalentTo(floorOptions.Floors);
    }
}
