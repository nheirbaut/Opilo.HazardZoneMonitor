using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.ValueObjects;

public sealed class SiteNameTests
{
    [Fact]
    public void From_ShouldCreateSiteName_WhenNameIsValid()
    {
        // Arrange
        var name = "ReactorFacility";

        // Act
        var siteName = SiteName.From(name);

        // Assert
        siteName.Value.Should().Be("REACTORFACILITY");
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void From_ShouldThrowValueObjectValidationException_WhenNameIsInvalid(string invalidName)
    {
        // Act
        var act = () => SiteName.From(invalidName);

        // Assert
        act.Should().Throw<Vogen.ValueObjectValidationException>();
    }

    [Fact]
    public void From_ShouldBeCaseInsensitive_WhenNamesDifferOnlyByCase()
    {
        // Arrange
        var lowerCase = SiteName.From("reactorfacility");
        var upperCase = SiteName.From("REACTORFACILITY");
        var mixedCase = SiteName.From("ReactorFacility");

        // Assert
        lowerCase.Should().Be(upperCase);
        upperCase.Should().Be(mixedCase);
        lowerCase.GetHashCode().Should().Be(upperCase.GetHashCode());
    }

    [Fact]
    public void JsonSerialize_ShouldRoundTrip_WhenNameIsValid()
    {
        // Arrange
        var name = SiteName.From("TestSite");

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(name);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<SiteName>(json);

        // Assert
        deserialized.Should().Be(name);
        json.Should().Be("\"TESTSITE\"");
    }

    [Fact]
    public void JsonDeserialize_ShouldThrow_WhenNameIsEmpty()
    {
        // Act
        var act = () => System.Text.Json.JsonSerializer.Deserialize<SiteName>("\"\"");

        // Assert
        act.Should().Throw<System.Text.Json.JsonException>();
    }
}
