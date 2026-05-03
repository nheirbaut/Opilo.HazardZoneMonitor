using Opilo.HazardZoneMonitor.Api.Features.Site;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Site;

public sealed class SiteOptionsValidatorTests
{
    private readonly SiteOptionsValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenNameIsProvided()
    {
        // Arrange
        var options = new SiteOptions { Name = SiteName.From("Reactor Facility Alpha") };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }
}
