using System.Net;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.HazardZones;

public sealed class HazardZoneConfigurationStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Api_ShouldStart_WhenHazardZoneConfigurationIsValid()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetAsync(
            new Uri("/api/v1/hazard-zones", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenHazardZoneNamesAreDuplicate()
    {
        // Arrange
        var hazardZoneOptions = new HazardZoneOptions
        {
            HazardZones =
            [
                HazardZoneConfigurationBuilder.BuildSimple(),
                HazardZoneConfigurationBuilder.Create().WithOutline(new Coordinate(2, 2), new Coordinate(3, 2), new Coordinate(2, 3)).Build()
            ]
        };

        var hostBuilder = factory.CreateHost()
            .WithHazardZoneConfiguration(hazardZoneOptions);

        // Act
        Action act = () => hostBuilder.Start();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

}
