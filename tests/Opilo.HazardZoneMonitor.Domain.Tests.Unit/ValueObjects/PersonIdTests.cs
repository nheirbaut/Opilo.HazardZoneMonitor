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
}
