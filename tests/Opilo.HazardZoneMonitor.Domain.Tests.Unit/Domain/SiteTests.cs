using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.Domain;

public sealed class SiteTests
{
    private const string ValidSiteName = "TestSite";

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        var act = () => new Site(null!, []);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string invalidName)
    {
        // Act & Assert
        var act = () => new Site(invalidName, []);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldCreateSite_WhenEmptyFloorsCollection()
    {
        // Act
        var site = new Site(ValidSiteName, []);

        // Assert
        site.Name.Should().Be(ValidSiteName);
    }
}

#pragma warning disable MA0048
public sealed class Site
#pragma warning restore MA0048
{
    public Site(string name, IList<Floor> floors)
    {
        Guard.Against.NullOrWhiteSpace(name);

        Name = name;
    }

    public string Name { get; }
}
