using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Tests.Common.TestUtilities.Builders;

internal static class OutlineBuilder
{
    public static IReadOnlyList<Coordinate> Rectangle(double minX, double minY, double maxX, double maxY) =>
    [
        new(minX, minY),
        new(maxX, minY),
        new(maxX, maxY),
        new(minX, maxY),
    ];

    public static IReadOnlyList<Coordinate> Triangle(
        double firstX,
        double firstY,
        double secondX,
        double secondY,
        double thirdX,
        double thirdY) =>
    [
        new(firstX, firstY),
        new(secondX, secondY),
        new(thirdX, thirdY),
    ];
}
