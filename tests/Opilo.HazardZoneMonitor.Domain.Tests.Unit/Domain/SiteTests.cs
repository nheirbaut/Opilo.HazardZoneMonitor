// ReSharper disable AccessToDisposedClosure

using Opilo.HazardZoneMonitor.Domain.Features.SiteManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities.Builders;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.Domain;

public sealed class SiteTests
{
    private static readonly SiteName s_validSiteName = SiteName.From("TestSite");

    [Fact]
    public void Constructor_ShouldAcceptSiteName_WhenValidNameIsProvided()
    {
        // Arrange
        var siteName = SiteName.From("TestSite");

        // Act
        var site = new Site(siteName, []);

        // Assert
        site.Name.Should().Be(siteName);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenFloorsIsNull()
    {
        // Act
        var act = () => new Site(s_validSiteName, null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>()
            .WithParameterName("floors");
    }

    [Fact]
    public void Constructor_ShouldCreateSite_WhenEmptyFloorsCollection()
    {
        // Act
        var site = new Site(s_validSiteName, []);

        // Assert
        site.Name.Should().Be(s_validSiteName);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenDuplicateFloorsAreProvided()
    {
        // Arrange
        using var floor = FloorBuilder.BuildSimple();

        // Act
        var act = () => new Site(s_validSiteName, [floor, floor]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenFloorsHaveSameName()
    {
        // Arrange
        using var floor1 = FloorBuilder.Create().WithName(FloorName.From("FloorA")).Build();
        using var floor2 = FloorBuilder.Create().WithName(FloorName.From("FloorA")).Build();

        // Act
        var act = () => new Site(s_validSiteName, [floor1, floor2]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
