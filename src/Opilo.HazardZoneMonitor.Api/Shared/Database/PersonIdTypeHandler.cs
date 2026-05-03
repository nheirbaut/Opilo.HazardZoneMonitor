using System.Data;
using Dapper;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Api.Shared.Database;

public sealed class PersonIdTypeHandler : SqlMapper.TypeHandler<PersonId>
{
    public override void SetValue(IDbDataParameter parameter, PersonId value)
    {
        parameter.Value = value.Value;
    }

    public override PersonId Parse(object value)
    {
        return value switch
        {
            Guid guid => PersonId.From(guid),
            string str => PersonId.From(Guid.Parse(str)),
            _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to PersonId")
        };
    }
}
