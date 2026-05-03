namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class FloorNameTests
{
    [Fact]
    public void From_ShouldCreateFloorName_WhenNameIsValid()
    {
        // Arrange
        var name = "GroundFloor";

        // Act
        var floorName = FloorName.From(name);

        // Assert
        floorName.Value.Should().Be("GROUNDFLOOR");
    }
}
