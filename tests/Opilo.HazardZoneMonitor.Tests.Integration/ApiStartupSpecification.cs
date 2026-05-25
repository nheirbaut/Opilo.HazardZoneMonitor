using System.Net;
namespace Opilo.HazardZoneMonitor.Tests.Integration;

public sealed class ApiStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Api_ShouldStart_WhenConfigurationIsValid()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
