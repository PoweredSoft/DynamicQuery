using System;
using System.Linq;
using PoweredSoft.Types.Interface;

namespace PoweredSoft.DynamicQuery.TypeConverters
{
    public class StringToTimeSpanConverter : ITypeConverter
    {
        public bool CanConvert(Type source, Type destination) =>
            source == typeof(string) && new[] {typeof(TimeSpan), typeof(TimeSpan?)}.Any(t => t == destination);

        public object Convert(object source, Type destination)
        {
            if (TimeSpan.TryParse(source?.ToString(), out var value)) return value;

            return null;
        }
    }
}