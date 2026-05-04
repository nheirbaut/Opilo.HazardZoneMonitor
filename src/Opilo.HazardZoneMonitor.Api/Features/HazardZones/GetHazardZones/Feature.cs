using System.ComponentModel;
using Ardalis.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;
using Opilo.HazardZoneMonitor.Api.Shared.Infrastructure;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones.GetHazardZones;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new HazardZoneNameJsonConverter()));

        services.AddSingleton<IValidateOptions<HazardZoneOptions>, HazardZoneOptionsValidator>();
        services.AddOptions<HazardZoneOptions>()
            .Configure<IConfiguration>((options, configuredConfiguration) =>
            {
                ValidateHazardZoneNameConfiguration(configuredConfiguration);
                configuredConfiguration.GetSection(nameof(HazardZoneOptions)).Bind(options);
            })
            .ValidateOnStart();

        services.AddScoped<IQueryHandler<Query, GetHazardZonesResponse>, Handler>();
    }

    private static void ValidateHazardZoneNameConfiguration(IConfiguration configuration)
    {
        var hazardZoneNameConverter = TypeDescriptor.GetConverter(typeof(HazardZoneName));
        var hazardZonesSection = configuration.GetSection($"{nameof(HazardZoneOptions)}:{nameof(HazardZoneOptions.HazardZones)}");

        foreach (var hazardZoneSection in hazardZonesSection.GetChildren())
        {
            try
            {
                hazardZoneNameConverter.ConvertFromInvariantString(hazardZoneSection[nameof(HazardZoneConfiguration.Name)] ?? string.Empty);
            }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException)
            {
                throw new InvalidOperationException($"Failed to convert configuration value at '{hazardZoneSection.Path}:{nameof(HazardZoneConfiguration.Name)}' to type '{typeof(HazardZoneName)}'.", exception);
            }
        }
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/hazard-zones", async Task<Results<Ok<GetHazardZonesResponse>, StatusCodeHttpResult>> (
            IQueryHandler<Query, GetHazardZonesResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new Query(), cancellationToken);

            if (result.Status != ResultStatus.Ok)
            {
                return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
            }

            return TypedResults.Ok(result.Value);
        });
    }
}
