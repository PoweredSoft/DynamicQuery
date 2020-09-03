using System;
using System.Linq;
using PoweredSoft.Types.Interface;

namespace PoweredSoft.DynamicQuery
{
    public class EnumConverter : ITypeConverter
    {
        public bool CanConvert(Type source, Type destination)
        {
            return new[] {typeof(string), typeof(int), typeof(Int64), typeof(long)}
                .Any(t => t == source) && destination.BaseType == typeof(Enum);
        }


        public object Convert(object source, Type destination)
        {
            return Enum.Parse(destination, source.ToString());
        }
    }
}