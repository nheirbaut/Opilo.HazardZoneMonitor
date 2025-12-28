using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

namespace Opilo.HazardZoneMonitor.Tests.Unit.Features.FloorManagement;

public sealed class FloorRegistryTests
{
    [Fact]
    public void GetAllFloors_ShouldReturnEmptyList_WhenConfigurationContainsNoFloors()
    {
        // Arrange
        var options = Options.Create(new FloorConfiguration { Floors = [] });
        var registry = new FloorRegistry(options);

        // Act
        var floors = registry.GetAllFloors();

        // Assert
        floors.Should().BeEmpty();
    }

    [Fact]
    public void GetAllFloors_ShouldReturnFloors_WhenConfigurationContainsFloors()
    {
        // Arrange
        var configuredFloors = new List<FloorDto>
        {
            new("floor-1", "Ground Floor"),
            new("floor-2", "First Floor")
        };
        var options = Options.Create(new FloorConfiguration { Floors = configuredFloors });
        var registry = new FloorRegistry(options);

        // Act
        var floors = registry.GetAllFloors();

        // Assert
        floors.Should().BeEquivalentTo(configuredFloors);
    }
}
