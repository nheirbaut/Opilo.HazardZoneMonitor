using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class FloorNameTests
{
    [Fact]
    public void From_ShouldCreateFloorName_WhenNameIsValid()
    {
        // Arrange
        var name = "GroundFloor";

        // Act
        var floorName = FloorName.From(name);

        // Assert
        floorName.Value.Should().Be("GROUNDFLOOR");
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void From_ShouldThrowValueObjectValidationException_WhenNameIsInvalid(string invalidName)
    {
        // Act
        var act = () => FloorName.From(invalidName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldBeCaseInsensitive_WhenNamesDifferOnlyByCase()
    {
        // Arrange
        var lowerCase = FloorName.From("groundfloor");
        var upperCase = FloorName.From("GROUNDFLOOR");
        var mixedCase = FloorName.From("GroundFloor");

        // Assert
        lowerCase.Should().Be(upperCase);
        upperCase.Should().Be(mixedCase);
        lowerCase.GetHashCode().Should().Be(upperCase.GetHashCode());
    }

    [Fact]
    public void JsonSerialize_ShouldRoundTrip_WhenNameIsValid()
    {
        // Arrange
        var name = FloorName.From("TestFloor");

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(name);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<FloorName>(json);

        // Assert
        deserialized.Should().Be(name);
        json.Should().Be("\"TESTFLOOR\"");
    }

    [Fact]
    public void JsonDeserialize_ShouldThrow_WhenNameIsEmpty()
    {
        // Act
        var act = () => System.Text.Json.JsonSerializer.Deserialize<FloorName>("\"\"");

        // Assert
        act.Should().Throw<System.Text.Json.JsonException>();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenFloorNamesHaveDifferentValues()
    {
        // Arrange
        var floorName1 = FloorName.From("FloorOne");
        var floorName2 = FloorName.From("FloorTwo");

        // Act
        var result = floorName1 == floorName2;

        // Assert
        result.Should().BeFalse();
    }
}
