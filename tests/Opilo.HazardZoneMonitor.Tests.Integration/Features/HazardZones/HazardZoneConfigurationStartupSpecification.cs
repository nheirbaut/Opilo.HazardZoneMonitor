using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class HazardZoneConfigurationStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private WebApplicationFactory<IApiMarker>? _customFactory;

    [Fact]
    public async Task Api_ShouldStart_WhenHazardZoneConfigurationIsValid()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            new Uri("/api/v1/hazard-zones", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenHazardZoneNameIsEmpty()
    {
        // Arrange
        var hazardZoneOptions = new HazardZoneOptions
        {
            HazardZones = [new HazardZoneConfiguration(string.Empty, [new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)], TimeSpan.Zero, TimeSpan.Zero)]
        };

        _customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(hazardZoneOptions.ToConfigurationDictionary());
            });
        });

        // Act
        Action act = () => _customFactory.CreateClient();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenHazardZoneNamesAreDuplicate()
    {
        // Arrange
        var hazardZoneOptions = new HazardZoneOptions
        {
            HazardZones =
            [
                new HazardZoneConfiguration("Zone", [new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)], TimeSpan.Zero, TimeSpan.Zero),
                new HazardZoneConfiguration("Zone", [new PointConfiguration(2, 2), new PointConfiguration(3, 2), new PointConfiguration(2, 3)], TimeSpan.Zero, TimeSpan.Zero)
            ]
        };

        _customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(hazardZoneOptions.ToConfigurationDictionary());
            });
        });

        // Act
        Action act = () => _customFactory.CreateClient();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    public void Dispose()
    {
        _customFactory?.Dispose();
    }
}
