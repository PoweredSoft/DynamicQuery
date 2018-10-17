using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IAggregateResult
    {
        string Path { get; set; }
        AggregateType Type { get; set; }
        object Value { get; set; }
    }

    public interface IQueryResult
    {
        long Count { get; }
        List<IAggregateResult> Aggregates { get; }
    }

    public interface IQueryResultSimple : IQueryResult
    {
        List<object> Data { get; }
    }

    public interface IQueryResultGrouped : IQueryResult
    {
        List<IQueryResult> Data { get;  }
    }
}
