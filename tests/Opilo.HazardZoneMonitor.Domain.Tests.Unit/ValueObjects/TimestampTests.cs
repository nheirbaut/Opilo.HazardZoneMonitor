using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class TimestampTests
{
    [Fact]
    public void From_ShouldCreateTimestamp_WhenValueIsProvided()
    {
        // Arrange
        var value = DateTime.UnixEpoch;

        // Act
        var timestamp = Timestamp.From(value);

        // Assert
        timestamp.Value.Should().Be(value);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var first = Timestamp.From(DateTime.UnixEpoch);
        var second = Timestamp.From(DateTime.UnixEpoch);

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var timestamp = Timestamp.From(DateTime.UnixEpoch);

        // Assert
        timestamp.Should().BeAssignableTo<ValueObject>();
    }
}
