using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api;
using Opilo.HazardZoneMonitor.Api.Features.Floors;
using Opilo.HazardZoneMonitor.Api.Shared.Configuration;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;
using Opilo.HazardZoneMonitor.Tests.Integration.Shared;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.Floors;

public sealed class FloorConfigurationStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private WebApplicationFactory<IApiMarker>? _customFactory;

    [Fact]
    public async Task Api_ShouldStart_WhenFloorConfigurationIsValid()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(
            new Uri("/api/v1/floors", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenFloorNameIsEmpty()
    {
        // Arrange
        var floorOptions = new FloorOptions
        {
            Floors = [FloorConfigurationBuilder.Create().WithName(string.Empty).Build()]
        };

        _customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(floorOptions.ToConfigurationDictionary());
            });
        });

        // Act
        Action act = () => _customFactory.CreateClient();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenFloorNamesAreDuplicate()
    {
        // Arrange
        var floorOptions = new FloorOptions
        {
            Floors =
            [
                FloorConfigurationBuilder.Create().WithOutline(new PointConfiguration(0, 0), new PointConfiguration(1, 0), new PointConfiguration(0, 1)).Build(),
                FloorConfigurationBuilder.Create().WithOutline(new PointConfiguration(2, 2), new PointConfiguration(3, 2), new PointConfiguration(2, 3)).Build()
            ]
        };

        _customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(floorOptions.ToConfigurationDictionary());
            });
        });

        // Act
        Action act = () => _customFactory.CreateClient();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenFloorOutlineHasFewerThanThreePoints()
    {
        // Arrange
        var floorOptions = new FloorOptions
        {
            Floors = [FloorConfigurationBuilder.Create().WithOutline(new PointConfiguration(0, 0), new PointConfiguration(1, 1)).Build()]
        };

        _customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(floorOptions.ToConfigurationDictionary());
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
