using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
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

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void From_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act
        var act = () => SiteName.From(invalidName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_WhenValuesAreEqual()
    {
        // Arrange
        var first = SiteName.From("Site");
        var second = SiteName.From("site");

        // Act & Assert
        (first == second).Should().BeTrue();
        (first != second).Should().BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparedToNull()
    {
        // Arrange
        var name = SiteName.From("Site");

        // Act & Assert
        name.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void IsAssignableToValueObject_ShouldBeTrue()
    {
        // Arrange
        var name = SiteName.From("Site");

        // Assert
        name.Should().BeAssignableTo<ValueObject>();
    }
}
