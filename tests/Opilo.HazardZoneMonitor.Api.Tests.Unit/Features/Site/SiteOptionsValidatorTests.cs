using Opilo.HazardZoneMonitor.Api.Features.Site;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.Site;

public sealed class SiteOptionsValidatorTests
{
    private readonly SiteOptionsValidator _validator = new();

    [Fact]
    public void Validate_ShouldReturnSuccess_WhenNameIsProvided()
    {
        // Arrange
        var options = new SiteOptions { Name = "Reactor Facility Alpha" };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldReturnFailure_WhenNameIsNullOrWhitespace(string? invalidName)
    {
        // Arrange
        var options = new SiteOptions { Name = invalidName };

        // Act
        var result = _validator.Validate(string.Empty, options);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().ContainSingle();
    }
}
