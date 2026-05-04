using System.Diagnostics.CodeAnalysis;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.Domain;

[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code",
    Justification = "Tests intentionally exercise null and equality boundaries that the analyzer infers as constant.")]
public sealed class ValueObjectTests
{
    [Fact]
    public void Equals_ShouldReturnTrue_WhenComponentsAreEqual()
    {
        // Arrange
        var first = new TestValueObject("a", 1);
        var second = new TestValueObject("a", 1);

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComponentsDiffer()
    {
        // Arrange
        var first = new TestValueObject("a", 1);
        var second = new TestValueObject("a", 2);

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenOtherIsNull()
    {
        // Arrange
        var valueObject = new TestValueObject("a", 1);

        // Act
        var result = valueObject.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenTypesDiffer()
    {
        // Arrange
        var first = new TestValueObject("a", 1);
        var second = new OtherTestValueObject("a", 1);

        // Act
        var result = first.Equals((object)second);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_ShouldReturnFalse_WhenObjectIsDifferentType()
    {
        // Arrange
        var valueObject = new TestValueObject("a", 1);

        // Act
        var result = valueObject.Equals("a");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldBeEqual_WhenComponentsAreEqual()
    {
        // Arrange
        var first = new TestValueObject("a", 1);
        var second = new TestValueObject("a", 1);

        // Act & Assert
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldDiffer_WhenComponentsDiffer()
    {
        // Arrange
        var first = new TestValueObject("a", 1);
        var second = new TestValueObject("a", 2);

        // Act & Assert
        first.GetHashCode().Should().NotBe(second.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenBothAreNull()
    {
        // Arrange
        TestValueObject? first = null;
        TestValueObject? second = null;

        // Act
        var result = first == second;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenOnlyOneIsNull()
    {
        // Arrange
        TestValueObject? first = null;
        var second = new TestValueObject("a", 1);

        // Act & Assert
        (first == second).Should().BeFalse();
        (second == first).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenComponentsAreEqual()
    {
        // Arrange
        var first = new TestValueObject("a", 1);
        var second = new TestValueObject("a", 1);

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldHandleNullComponents()
    {
        // Arrange
        var first = new TestValueObject(null, 1);
        var second = new TestValueObject(null, 1);

        // Act & Assert
        first.Equals(second).Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    private sealed class TestValueObject(string? text, int number) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return text;
            yield return number;
        }
    }

    private sealed class OtherTestValueObject(string? text, int number) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return text;
            yield return number;
        }
    }
}
