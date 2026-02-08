using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration;

public sealed class ApiStartupTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Api_ShouldStart_WhenConfigurationIsValid()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/", UriKind.Relative));

        // Assert
        response.Should().NotBeNull();
    }
}
