using Dapper;
using Microsoft.Data.Sqlite;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

internal static class DatabaseInitializer
{
    public static void EnsurePersonMovementsTable(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        using SqliteConnection connection = new(connectionString);
        connection.Open();

        const string sql = """
            CREATE TABLE IF NOT EXISTS PersonMovements (
                Id TEXT NOT NULL PRIMARY KEY,
                PersonId TEXT NOT NULL,
                X REAL NOT NULL,
                Y REAL NOT NULL,
                RegisteredAt TEXT NOT NULL
            )
            """;

        connection.Execute(sql);
    }
}
