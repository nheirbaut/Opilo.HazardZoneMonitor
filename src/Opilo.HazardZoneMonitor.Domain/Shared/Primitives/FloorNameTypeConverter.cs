using System.ComponentModel;
using System.Globalization;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class FloorNameTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string floorName)
        {
            return FloorName.From(floorName);
        }

        return base.ConvertFrom(context, culture, value);
    }
}
