using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
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

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void From_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act
        var act = () => FloorName.From(invalidName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var first = FloorName.From("Floor");
        var second = FloorName.From("floor");

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparedToNull()
    {
        // Arrange
        var name = FloorName.From("Floor");

        // Act & Assert
        name.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var name = FloorName.From("Floor");

        // Assert
        name.Should().BeAssignableTo<ValueObject>();
    }
}
