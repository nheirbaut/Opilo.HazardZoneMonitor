namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared.Hosting;

internal static class TestHostDefaults
{
    public static IDictionary<string, string?> CreateSettings(string databasePath) =>
        new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["ConnectionStrings:DefaultConnection"] = $"Data Source={databasePath}",
            ["SiteOptions:Name"] = "Test Site",
        };
}
