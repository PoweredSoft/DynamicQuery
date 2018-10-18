using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery
{
    public class QueryCriteria : IQueryCriteria
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public List<ISort> Sorts { get; set; } = new List<ISort>();
        public List<IFilter> Filters { get; set; } = new List<IFilter>();
        public List<IGroup> Groups { get; set; } = new List<IGroup>();
        public List<IAggregate> Aggregates { get; set; } = new List<IAggregate>();
    }
}
