using System.Data;
using Microsoft.Data.Sqlite;

namespace Opilo.HazardZoneMonitor.Api.Shared.Database;

internal sealed class SqliteDbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection Create() => new SqliteConnection(connectionString);
}
