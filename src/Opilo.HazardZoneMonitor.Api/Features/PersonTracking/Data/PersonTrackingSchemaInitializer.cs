using Dapper;
using Opilo.HazardZoneMonitor.Api.Shared.Database;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking.Data;

internal sealed class PersonTrackingSchemaInitializer(IDbConnectionFactory connectionFactory) : ISchemaInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.Create();
        connection.Open();

        const string sql = """
            CREATE TABLE IF NOT EXISTS PersonMovements (
                Id TEXT NOT NULL PRIMARY KEY,
                PersonId TEXT NOT NULL,
                Coordinate TEXT NOT NULL,
                RegisteredAt TEXT NOT NULL
            )
            """;

        CommandDefinition command = new(sql, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);
    }
}
