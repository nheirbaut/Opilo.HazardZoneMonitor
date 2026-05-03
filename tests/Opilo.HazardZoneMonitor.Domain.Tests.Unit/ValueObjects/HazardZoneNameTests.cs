using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;
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
        hazardZoneName.Value.Should().Be(name);
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void From_ShouldThrowValueObjectValidationException_WhenNameIsInvalid(string invalidName)
    {
        // Act
        var act = () => HazardZoneName.From(invalidName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }
}
