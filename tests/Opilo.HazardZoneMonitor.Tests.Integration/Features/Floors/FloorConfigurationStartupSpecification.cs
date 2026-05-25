using System.Net;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Features.Floors;

public sealed class FloorConfigurationStartupSpecification(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Api_ShouldStart_WhenFloorConfigurationIsValid()
    {
        // Arrange
        await using var host = factory.CreateHost().Start();
        var client = host.CreateClient();

        // Act
        var response = await client.GetAsync(
            new Uri("/api/v1/floors", UriKind.Relative),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public void Api_ShouldFailToStart_WhenFloorNameIsEmpty()
    {
        // Arrange
        var hostBuilder = factory.CreateHost()
            .WithSetting("FloorOptions:Floors:0:Name", string.Empty)
            .WithSetting("FloorOptions:Floors:0:Outline:0:X", "0")
            .WithSetting("FloorOptions:Floors:0:Outline:0:Y", "0")
            .WithSetting("FloorOptions:Floors:0:Outline:1:X", "10")
            .WithSetting("FloorOptions:Floors:0:Outline:1:Y", "0")
            .WithSetting("FloorOptions:Floors:0:Outline:2:X", "0")
            .WithSetting("FloorOptions:Floors:0:Outline:2:Y", "10");

        // Act
        Action act = () => hostBuilder.Start();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenFloorNamesAreDuplicate()
    {
        // Arrange
        var floorOptions = new FloorOptions
        {
            Floors =
            [
                FloorConfigurationBuilder.Create().WithOutline(new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1)).Build(),
                FloorConfigurationBuilder.Create().WithOutline(new Coordinate(2, 2), new Coordinate(3, 2), new Coordinate(2, 3)).Build()
            ]
        };

        var hostBuilder = factory.CreateHost()
            .WithFloorConfiguration(floorOptions);

        // Act
        Action act = () => hostBuilder.Start();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void Api_ShouldThrowOptionsValidationException_WhenFloorOutlineHasFewerThanThreePoints()
    {
        // Arrange
        var floorOptions = new FloorOptions
        {
            Floors = [FloorConfigurationBuilder.Create().WithOutline(new Coordinate(0, 0), new Coordinate(1, 1)).Build()]
        };

        var hostBuilder = factory.CreateHost()
            .WithFloorConfiguration(floorOptions);

        // Act
        Action act = () => hostBuilder.Start();

        // Assert
        act.Should().Throw<OptionsValidationException>();
    }

}
