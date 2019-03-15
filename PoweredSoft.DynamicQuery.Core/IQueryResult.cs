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

    public interface IQueryResult<TRes>
    {
        List<IAggregateResult> Aggregates { get; }
        List<TRes> Data { get; }
    }

    public interface IGroupQueryResult<TRes> : IQueryResult<TRes>
    {
        string GroupPath { get; set; }
        object GroupValue { get; set; }
    }

    public interface IQueryExecutionResult<TRes> : IQueryResult<TRes>
    {
        long TotalRecords { get; set; }
        long? NumberOfPages { get; set; }
    }

   
}
