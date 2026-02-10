using Dapper;
using Microsoft.Data.Sqlite;
using Opilo.HazardZoneMonitor.Api.Shared.Features;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

public sealed class Feature : IFeature
{
    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=hazardzone.db";

        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        services.AddScoped(_ =>
        {
            SqliteConnection connection = new(connectionString);
            try
            {
                connection.Open();
            }
            catch
            {
                connection.Dispose();
                throw;
            }

            return connection;
        });
        services.AddScoped<IMovementsRepository, MovementsRepository>();

        DatabaseInitializer.EnsurePersonMovementsTable(connectionString);
    }

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        // Infrastructure feature — no endpoints to map.
    }
}
