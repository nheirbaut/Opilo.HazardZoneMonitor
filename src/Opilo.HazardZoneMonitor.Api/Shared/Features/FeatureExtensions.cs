using System.Reflection;

namespace Opilo.HazardZoneMonitor.Api.Shared.Features;

public static class FeatureExtensions
{
    public static IServiceCollection AddFeaturesFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        assembly.InvokeFeatureAction(feature => feature.AddServices(services));

        return services;
    }

    public static IEndpointRouteBuilder MapFeaturesFromAssembly(
        this IEndpointRouteBuilder app,
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(assembly);

        assembly.InvokeFeatureAction(feature => feature.MapEndpoints(app));

        return app;
    }

    private static void InvokeFeatureAction(this Assembly assembly, Action<IFeature> action)
    {
        var featureTypes = assembly
            .GetTypes()
            .Where(t =>
                !t.IsAbstract
                && typeof(IFeature).IsAssignableFrom(t));

        foreach (var featureType in featureTypes)
        {
            var feature = (IFeature)Activator.CreateInstance(featureType)!;
            action(feature);
        }
    }
}
