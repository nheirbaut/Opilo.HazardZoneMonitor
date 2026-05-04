using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class HazardZoneNameTests
{
    [Fact]
    public void From_ShouldCreateHazardZoneName_WhenNameIsValid()
    {
        // Arrange
        var name = "AssemblyLine";

        // Act
        var hazardZoneName = HazardZoneName.From(name);

        // Assert
        hazardZoneName.Value.Should().Be("AssemblyLine");
    }

    [Fact]
    public void From_ShouldBeCaseSensitive_WhenNamesDifferOnlyByCase()
    {
        // Arrange
        var lowerCase = HazardZoneName.From("assemblyline");
        var upperCase = HazardZoneName.From("ASSEMBLYLINE");
        var mixedCase = HazardZoneName.From("AssemblyLine");

        // Assert
        lowerCase.Should().NotBe(upperCase);
        upperCase.Should().NotBe(mixedCase);
        lowerCase.GetHashCode().Should().NotBe(upperCase.GetHashCode());
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void From_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act
        var act = () => HazardZoneName.From(invalidName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var first = HazardZoneName.From("Zone");
        var second = HazardZoneName.From("Zone");

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparedToNull()
    {
        // Arrange
        var name = HazardZoneName.From("Zone");

        // Act & Assert
        name.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var name = HazardZoneName.From("Zone");

        // Assert
        name.Should().BeAssignableTo<ValueObject>();
    }
}
