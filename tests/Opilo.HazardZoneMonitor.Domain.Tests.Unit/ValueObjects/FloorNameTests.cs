using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

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

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void From_ShouldThrowValueObjectValidationException_WhenNameIsInvalid(string invalidName)
    {
        // Act
        var act = () => FloorName.From(invalidName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldBeCaseInsensitive_WhenNamesDifferOnlyByCase()
    {
        // Arrange
        var lowerCase = FloorName.From("groundfloor");
        var upperCase = FloorName.From("GROUNDFLOOR");
        var mixedCase = FloorName.From("GroundFloor");

        // Assert
        lowerCase.Should().Be(upperCase);
        upperCase.Should().Be(mixedCase);
        lowerCase.GetHashCode().Should().Be(upperCase.GetHashCode());
    }
}
