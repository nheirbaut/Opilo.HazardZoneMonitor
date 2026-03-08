using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Site;
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
        var expectedSite = new SiteConfiguration("Reactor Facility Alpha");

        var siteOptions = new SiteOptions { Name = expectedSite.Name };

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
        var response = await client.GetFromJsonAsync<Response>(
            new Uri("/api/v1/site", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        // Assert
        response.Should().NotBeNull();
        response.Site.Should().BeEquivalentTo(expectedSite);
    }
}
