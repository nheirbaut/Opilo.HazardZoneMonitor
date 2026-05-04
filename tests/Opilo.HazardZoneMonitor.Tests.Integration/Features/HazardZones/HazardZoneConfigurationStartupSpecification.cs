using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;
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
