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
}

#pragma warning disable MA0048
public sealed class Site
#pragma warning restore MA0048
{
    public Site(string name)
    {
    }
}
