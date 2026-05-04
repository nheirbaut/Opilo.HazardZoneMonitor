using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class TimestampTests
{
    [Fact]
    public void Constructor_ShouldCreateTimestamp_WhenValueIsProvided()
    {
        // Arrange
        var value = DateTime.UnixEpoch;

        // Act
        var timestamp = new Timestamp(value);

        // Assert
        timestamp.Value.Should().Be(value);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var first = new Timestamp(DateTime.UnixEpoch);
        var second = new Timestamp(DateTime.UnixEpoch);

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var timestamp = new Timestamp(DateTime.UnixEpoch);

        // Assert
        timestamp.Should().BeAssignableTo<ValueObject>();
    }
}
