using System.Collections;

namespace Opilo.HazardZoneMonitor.Domain.Tests.Unit.TestUtilities;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes (instantiated by xUnit via [ClassData])
internal sealed class InvalidNames : IEnumerable<object[]>
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

#pragma warning restore CA1812
