using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class HazardZoneNameTests
{
    [Fact]
    public void From_ShouldCreateHazardZoneName_WhenNameIsValid()
    {
        // Arrange
        var name = "AssemblyLine";

        // Act
        var hazardZoneName = HazardZoneName.From(name);

        // Assert
        hazardZoneName.Value.Should().Be(name);
    }
}
