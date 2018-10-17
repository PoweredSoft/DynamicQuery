using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IQueryCriteria
    {
        int? Page { get; set; }
        int? PageSize { get; set; }
        List<ISort> Sorts { get; set; }
        List<IFilter> Filters { get; set; }
        List<IGroup> Groups { get; set; }
        List<IAggregate> Aggregates { get; set; }
    }
}
