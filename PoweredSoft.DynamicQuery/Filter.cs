using System;
using System.Collections.Generic;
using System.Text;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery
{
    public abstract class Filter : IFilter
    {
        public bool? And { get; set; }
        public FilterType Type { get; set; }
    }

    public class SimpleFilter : ISimpleFilter
    {
        public bool? And { get; set; }
        public bool? Not { get; set; }
        public FilterType Type { get; set; }
        public string Path { get; set; }
        public object Value { get; set; }
    }

    public class CompositeFilter : ICompositeFilter
    {
        public bool? And { get; set; }
        public FilterType Type { get; set; } = FilterType.Composite;
        public List<IFilter> Filters { get; set; }
    }
}
