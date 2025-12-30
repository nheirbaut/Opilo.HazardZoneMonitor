using System.Net;
using System.Net.Http.Json;
using Opilo.HazardZoneMonitor.Api.Features.FloorManagement;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.FloorManagement;

public sealed class GetFloorsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public GetFloorsEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetFloors_ShouldResponseWithoutFloors_WhenNoFloorsAreRegistered()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<GetFloorResponse>(new Uri("/api/v1/floors", UriKind.Relative));

        // Assert
        response.Should().NotBeNull();
        response.Floors.Should().NotBeNull();
        response.Floors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFloors_ShouldReturn200Ok_WhenCalled()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/v1/floors", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
