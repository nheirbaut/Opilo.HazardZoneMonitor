using System.Data;
using Dapper;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

internal sealed class PersonIdTypeHandler : SqlMapper.TypeHandler<PersonId>
{
    public override PersonId Parse(object value)
    {
        var guid = Guid.Parse(value as string ?? value.ToString() ?? string.Empty);

        return PersonId.From(guid);
    }

    public override void SetValue(IDbDataParameter parameter, PersonId? value)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        parameter.Value = value is null
            ? DBNull.Value
            : value.Value.ToString();
    }
}
