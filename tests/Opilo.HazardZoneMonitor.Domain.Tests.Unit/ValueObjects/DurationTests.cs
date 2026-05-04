using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class DurationTests
{
    [Fact]
    public void From_ShouldCreateDuration_WhenValueIsNotNegative()
    {
        // Arrange
        var value = TimeSpan.FromSeconds(5);

        // Act
        var duration = Duration.From(value);

        // Assert
        duration.Value.Should().Be(value);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenValueIsNegative()
    {
        // Act
        var act = () => Duration.From(TimeSpan.FromMilliseconds(-1));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var first = Duration.From(TimeSpan.FromSeconds(5));
        var second = Duration.From(TimeSpan.FromSeconds(5));

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var duration = Duration.From(TimeSpan.Zero);

        // Assert
        duration.Should().BeAssignableTo<ValueObject>();
    }
}
