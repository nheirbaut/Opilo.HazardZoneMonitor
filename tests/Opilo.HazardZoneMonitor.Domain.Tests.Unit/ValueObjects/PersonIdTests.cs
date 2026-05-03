using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

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
    public void From_ShouldThrowValueObjectValidationException_WhenGuidIsEmpty()
    {
        // Act
        var act = () => PersonId.From(Guid.Empty);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenPersonIdsHaveSameGuidValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var personId1 = PersonId.From(guid);
        var personId2 = PersonId.From(guid);

        // Act
        var result = personId1 == personId2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenPersonIdsHaveDifferentGuidValue()
    {
        // Arrange
        var personId1 = PersonId.From(Guid.NewGuid());
        var personId2 = PersonId.From(Guid.NewGuid());

        // Act
        var result = personId1 == personId2;

        // Assert
        result.Should().BeFalse();
    }
}
