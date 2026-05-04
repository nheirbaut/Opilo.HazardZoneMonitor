using System.Data;
using System.Text.Json;
using Dapper;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Shared.Database;

internal sealed class CoordinateTypeHandler : SqlMapper.TypeHandler<Coordinate>
{
    public override Coordinate Parse(object value)
    {
        var json = value as string ?? value.ToString() ?? string.Empty;
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        double x = root.GetProperty("X").GetDouble();
        double y = root.GetProperty("Y").GetDouble();

        return new Coordinate(x, y);
    }

    public override void SetValue(IDbDataParameter parameter, Coordinate? value)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        parameter.Value = value is null
            ? DBNull.Value
            : JsonSerializer.Serialize(value);
    }
}
