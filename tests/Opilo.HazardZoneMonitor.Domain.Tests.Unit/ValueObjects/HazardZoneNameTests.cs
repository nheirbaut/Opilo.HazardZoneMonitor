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
        hazardZoneName.Value.Should().Be("ASSEMBLYLINE");
    }

    [Fact]
    public void From_ShouldBeCaseInsensitive_WhenNamesDifferOnlyByCase()
    {
        // Arrange
        var lowerCase = HazardZoneName.From("assemblyline");
        var upperCase = HazardZoneName.From("ASSEMBLYLINE");
        var mixedCase = HazardZoneName.From("AssemblyLine");

        // Assert
        lowerCase.Should().Be(upperCase);
        upperCase.Should().Be(mixedCase);
        lowerCase.GetHashCode().Should().Be(upperCase.GetHashCode());
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

    [Fact]
    public void JsonSerialize_ShouldRoundTrip_WhenNameIsValid()
    {
        // Arrange
        var name = HazardZoneName.From("TestZone");

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(name);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<HazardZoneName>(json);

        // Assert
        deserialized.Should().Be(name);
        json.Should().Be("\"TESTZONE\"");
    }

    [Fact]
    public void JsonDeserialize_ShouldThrow_WhenNameIsEmpty()
    {
        // Act
        var act = () => System.Text.Json.JsonSerializer.Deserialize<HazardZoneName>("\"\"");

        // Assert
        act.Should().Throw<System.Text.Json.JsonException>();
    }

}
