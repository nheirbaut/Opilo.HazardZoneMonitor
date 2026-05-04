using System.ComponentModel;
using Ardalis.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Api.Shared.Cqrs;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors.GetFloors;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new FloorNameJsonConverter()));

        services.AddSingleton<IValidateOptions<FloorOptions>, FloorOptionsValidator>();
        services.AddOptions<FloorOptions>()
            .Configure<IConfiguration>((options, configuredConfiguration) =>
            {
                ValidateFloorNameConfiguration(configuredConfiguration);
                configuredConfiguration.GetSection(nameof(FloorOptions)).Bind(options);
            })
            .ValidateOnStart();

        services.AddScoped<IQueryHandler<Query, GetFloorsResponse>, Handler>();
    }

    private static void ValidateFloorNameConfiguration(IConfiguration configuration)
    {
        var floorNameConverter = TypeDescriptor.GetConverter(typeof(FloorName));
        var floorsSection = configuration.GetSection($"{nameof(FloorOptions)}:{nameof(FloorOptions.Floors)}");

        foreach (var floorSection in floorsSection.GetChildren())
        {
            try
            {
                floorNameConverter.ConvertFromInvariantString(floorSection[nameof(FloorConfiguration.Name)] ?? string.Empty);
            }
            catch (Exception exception) when (exception is ArgumentException or NotSupportedException)
            {
                throw new InvalidOperationException($"Failed to convert configuration value at '{floorSection.Path}:{nameof(FloorConfiguration.Name)}' to type '{typeof(FloorName)}'.", exception);
            }
        }
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/floors", async Task<Results<Ok<GetFloorsResponse>, StatusCodeHttpResult>> (
            IQueryHandler<Query, GetFloorsResponse> handler,
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
