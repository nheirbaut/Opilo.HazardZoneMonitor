using System.Net;
using System.Net.Http.Json;
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
}
