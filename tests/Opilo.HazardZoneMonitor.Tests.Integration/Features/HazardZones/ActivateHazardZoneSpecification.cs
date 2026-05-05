using System.Net;
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
}
