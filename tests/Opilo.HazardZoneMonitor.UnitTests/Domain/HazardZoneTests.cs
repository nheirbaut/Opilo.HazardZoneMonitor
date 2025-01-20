using System.Collections.ObjectModel;
using Opilo.HazardZoneMonitor.Domain.Entities;
using Opilo.HazardZoneMonitor.Domain.ValueObjects;
using Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

namespace Opilo.HazardZoneMonitor.UnitTests.Domain;

public sealed class HazardZoneTests
{
    private static readonly Outline s_validOutline = new(new([
        new Location(0, 0),
        new Location(4, 0),
        new Location(4, 4),
        new Location(0, 4)
    ]));

    private const string ValidHazardZoneName = "TestHazardZone";

    [Fact]
    public void Constructor_WhenNameIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HazardZone(null!, s_validOutline));
    }

    [Theory]
    [ClassData(typeof(InvalidNames))]
    public void Constructor_WhenNameIsInvalid_ThrowsArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HazardZone(invalidName, s_validOutline));
    }

    [Fact]
    public void Constructor_WhenOutlineIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HazardZone(ValidHazardZoneName, null!));
    }

    [Fact]
    public void Constructor_WhenValidNameAndOutlineGiven_CreatesInstance()
    {
        // Act
        var hazardZone = new HazardZone(ValidHazardZoneName, s_validOutline);

        // Assert
        Assert.Equal(ValidHazardZoneName, hazardZone.Name);
        Assert.Equal(s_validOutline, hazardZone.Outline);
    }
}
