using System.Net;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.Site;

public sealed class SiteConfigurationStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Api_ShouldStart_WhenSiteConfigurationIsValid()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetAsync(
            new Uri("/api/v1/site", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
