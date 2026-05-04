namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class Coordinate : ValueObject
{
    public double X { get; }
    public double Y { get; }

    public Coordinate(double x, double y)
    {
        X = x;
        Y = y;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}
