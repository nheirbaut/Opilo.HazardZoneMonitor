using Dapper;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.Data;
using Opilo.HazardZoneMonitor.Api.Shared.Database;
using Opilo.HazardZoneMonitor.Api.Shared.Features;
using Opilo.HazardZoneMonitor.Api.Shared.Infrastructure;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=hazardzone.db";

        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new CoordinateTypeHandler());
        SqlMapper.AddTypeHandler(new PersonIdTypeHandler());

        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new PersonIdJsonConverter()));

        services.AddSingleton<IDbConnectionFactory>(_ => new SqliteDbConnectionFactory(connectionString));
        services.AddScoped<IMovementsRepository, MovementsRepository>();
        services.AddSingleton<ISchemaInitializer, PersonTrackingSchemaInitializer>();
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Infrastructure feature — no endpoints to map.
    }
}
