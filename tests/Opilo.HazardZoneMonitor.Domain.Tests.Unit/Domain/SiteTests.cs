using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.Domain;

public sealed class SiteTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        var act = () => new Site(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act & Assert
        var act = () => new Site(invalidName);
        act.Should().Throw<ArgumentException>();
    }
}

#pragma warning disable MA0048
public sealed class Site
#pragma warning restore MA0048
{
    public Site(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
    }
}
