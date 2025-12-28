using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Opilo.HazardZoneMonitor.Api;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.FloorManagement;

public sealed class GetFloorsEndpointTests : IDisposable
{
    private readonly WebApplicationFactory<IApiMarker> _factory;

    public GetFloorsEndpointTests()
    {
        _factory = new WebApplicationFactory<IApiMarker>();
    }

    [Fact]
    public async Task GetFloors_ShouldReturn200_WithEmptyArray()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/v1/floors", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
