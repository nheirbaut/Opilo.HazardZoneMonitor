using System.Data;
using Dapper;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

internal sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        return Guid.Parse(value as string ?? value.ToString() ?? string.Empty);
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        parameter.Value = value.ToString();
    }
}
