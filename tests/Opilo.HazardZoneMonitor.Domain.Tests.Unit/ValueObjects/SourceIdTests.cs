using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class SourceIdTests
{
    [Fact]
    public void From_ShouldCreateSourceId_WhenValueIsValid()
    {
        // Arrange
        var value = "ext-src";

        // Act
        var sourceId = SourceId.From(value);

        // Assert
        sourceId.Value.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void From_ShouldThrowArgumentException_WhenValueIsInvalid(string invalidValue)
    {
        // Act
        var act = () => SourceId.From(invalidValue);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var first = SourceId.From("ext-src");
        var second = SourceId.From("ext-src");

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var sourceId = SourceId.From("ext-src");

        // Assert
        sourceId.Should().BeAssignableTo<ValueObject>();
    }
}
