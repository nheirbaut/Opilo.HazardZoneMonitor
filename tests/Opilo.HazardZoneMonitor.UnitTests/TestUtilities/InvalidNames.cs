using System.Collections;

namespace Opilo.HazardZoneMonitor.UnitTests.TestUtilities;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1515:AvoidUnnecessaryUsing")]
public class InvalidNames : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [""];
        yield return [" "];
        yield return ["\t"];
        yield return ["\r"];
        yield return ["\n"];
        yield return [" \t"];
        yield return [" \r"];
        yield return [" \n"];
        yield return ["\t "];
        yield return ["\r "];
        yield return ["\n "];
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
