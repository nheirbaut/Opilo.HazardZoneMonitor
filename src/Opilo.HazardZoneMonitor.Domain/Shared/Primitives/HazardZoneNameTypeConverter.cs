using System.ComponentModel;
using System.Globalization;

namespace Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

public sealed class HazardZoneNameTypeConverter : TypeConverter
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
        if (value is string hazardZoneName)
        {
            return HazardZoneName.From(hazardZoneName);
        }

        return base.ConvertFrom(context, culture, value);
    }
}
