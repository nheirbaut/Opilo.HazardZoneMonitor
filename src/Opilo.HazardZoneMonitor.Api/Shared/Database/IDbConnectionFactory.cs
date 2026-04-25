using System.Data;

namespace Opilo.HazardZoneMonitor.Api.Shared.Database;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}
