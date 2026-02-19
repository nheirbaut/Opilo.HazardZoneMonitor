// ReSharper disable AccessToDisposedClosure

using Ardalis.GuardClauses;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;
using Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities.Builders;

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

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenDuplicateFloorsAreProvided()
    {
        // Arrange
        using var floor = FloorBuilder.BuildSimple();

        // Act
        var act = () => new Site(ValidSiteName, [floor, floor]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenFloorsHaveSameName()
    {
        // Arrange
        using var floor1 = FloorBuilder.Create().WithName("FloorA").Build();
        using var floor2 = FloorBuilder.Create().WithName("FloorA").Build();

        // Act
        var act = () => new Site(ValidSiteName, [floor1, floor2]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}

#pragma warning disable MA0048
public sealed class Site
#pragma warning restore MA0048
{
    public Site(string name, IList<Floor> floors)
    {
        Guard.Against.NullOrWhiteSpace(name);

        var floorList = floors.ToList();
        Guard.Against.DuplicateFloor(floorList, nameof(floors));

        Name = name;
    }

    public string Name { get; }
}

#pragma warning disable MA0048
public static class FloorGuards
#pragma warning restore MA0048
{
    public static void DuplicateFloor(
        this IGuardClause guardClause,
        IReadOnlyCollection<Floor> floors,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(floors);

        var names = floors.Select(f => f.Name).ToList();
        var distinctNames = names.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (distinctNames != names.Count)
        {
            throw new ArgumentException("Duplicate Floor names are not allowed.", parameterName);
        }
    }
}
