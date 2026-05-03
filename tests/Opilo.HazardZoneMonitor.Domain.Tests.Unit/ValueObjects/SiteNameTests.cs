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
}
