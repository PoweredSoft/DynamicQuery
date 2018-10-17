using System.Collections.Generic;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface ICompositeFilter : IFilter
    {
        List<IFilter> Filters { get; set; }
    }
}
