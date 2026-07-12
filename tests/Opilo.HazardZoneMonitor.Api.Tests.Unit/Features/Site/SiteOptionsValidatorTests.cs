using Opilo.HazardZoneMonitor.Api.Features.Site.Configuration;
using Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Site;

public sealed class SiteOptionsValidatorTests
{
    private readonly SiteOptionsValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenNameIsProvided()
    {
        // Arrange
        var options = SiteOptionsBuilder.Create().WithName("Reactor Facility Alpha").Build();

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }
}
