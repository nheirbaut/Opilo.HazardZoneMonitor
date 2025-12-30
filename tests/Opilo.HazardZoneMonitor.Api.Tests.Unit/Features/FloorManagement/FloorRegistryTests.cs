using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.FloorManagement;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.FloorManagement;

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
}
