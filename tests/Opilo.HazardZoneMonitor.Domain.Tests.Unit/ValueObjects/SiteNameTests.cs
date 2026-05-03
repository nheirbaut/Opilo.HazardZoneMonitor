using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

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
}
