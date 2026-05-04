using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class PersonIdTests
{
    [Fact]
    public void From_ShouldCreatePersonId_WhenGuidIsValid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var personId = PersonId.From(guid);

        // Assert
        personId.Value.Should().Be(guid);
    }

    [Fact]
    public void From_ShouldThrowArgumentException_WhenGuidIsEmpty()
    {
        // Act
        var act = () => PersonId.From(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var first = PersonId.From(guid);
        var second = PersonId.From(guid);

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnFalse_WhenValuesDiffer()
    {
        // Arrange
        var first = PersonId.From(Guid.NewGuid());
        var second = PersonId.From(Guid.NewGuid());

        // Act & Assert
        (first == second).Should().BeFalse();
        (first != second).Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparedToNull()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());

        // Act & Assert
        personId.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameValue_WhenPersonIdsAreEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var first = PersonId.From(guid);
        var second = PersonId.From(guid);

        // Assert
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnGuidStringRepresentation()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var personId = PersonId.From(guid);

        // Act
        var result = personId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());

        // Assert
        personId.Should().BeAssignableTo<ValueObject>();
    }
}
