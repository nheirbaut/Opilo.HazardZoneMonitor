using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class CapacityTests
{
    [Fact]
    public void From_ShouldCreateCapacity_WhenValueIsNotNegative()
    {
        // Arrange
        var value = 3;

        // Act
        var capacity = Capacity.From(value);

        // Assert
        capacity.Value.Should().Be(value);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenValueIsNegative()
    {
        // Act
        var act = () => Capacity.From(-1);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var first = Capacity.From(3);
        var second = Capacity.From(3);

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var capacity = Capacity.From(0);

        // Assert
        capacity.Should().BeAssignableTo<ValueObject>();
    }
}
