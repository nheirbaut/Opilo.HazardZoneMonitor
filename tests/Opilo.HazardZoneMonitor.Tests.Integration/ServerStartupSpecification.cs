using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration;

public sealed class ServerStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private WebApplicationFactory<IApiMarker>? _customFactory;

    [Fact]
    public void Api_ShouldFailToStart_WhenSiteConfigurationIsInvalid()
    {
        // Arrange
        _customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    ["SiteOptions:Name"] = string.Empty,
                });
            });
        });

        // Act
        Action act = () => _customFactory.CreateClient();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    public void Dispose()
    {
        _customFactory?.Dispose();
    }
}
