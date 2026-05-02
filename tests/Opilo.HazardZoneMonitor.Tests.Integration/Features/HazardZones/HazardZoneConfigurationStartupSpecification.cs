using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class HazardZoneConfigurationStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
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

        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(hazardZoneOptions.ToConfigurationDictionary());
            });
        });

        // Act
        Action act = () => customFactory.CreateClient();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }
}
