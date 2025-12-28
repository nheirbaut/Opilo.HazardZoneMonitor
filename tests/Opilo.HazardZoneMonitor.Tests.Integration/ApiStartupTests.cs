using Microsoft.AspNetCore.Mvc.Testing;
using Opilo.HazardZoneMonitor.Api;

namespace Opilo.HazardZoneMonitor.Tests.Integration;

public sealed class ApiStartupTests : IDisposable
{
    private readonly WebApplicationFactory<IApiMarker> _factory;

    public ApiStartupTests()
    {
        _factory = new WebApplicationFactory<IApiMarker>();
    }

    [Fact]
    public async Task Api_ShouldStart_WhenConfigurationIsValid()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/", UriKind.Relative));

        // Assert
        response.Should().NotBeNull();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
