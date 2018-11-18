using System;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IFilter
    {
        bool? And { get; set; }
        FilterType Type { get; set; }
    }
}
