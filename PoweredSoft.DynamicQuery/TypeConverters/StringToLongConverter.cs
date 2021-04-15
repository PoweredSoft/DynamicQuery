using System;
using PoweredSoft.Types.Interface;

namespace PoweredSoft.DynamicQuery.TypeConverters
{
    public class StringToLongConverter : ITypeConverter
    {
        public bool CanConvert(Type source, Type destination) =>
            source == typeof(string) && (destination == typeof(long?) || destination == typeof(long));

        public object Convert(object source, Type destination)
        {
            if (source != null) return long.Parse(source.ToString() ?? "0");
            if (destination == typeof(long?)) return null;
            return 0;
        }
    }
}