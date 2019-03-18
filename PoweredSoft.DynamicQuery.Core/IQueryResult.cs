using System;
using System.Collections.Generic;
using System.Linq;
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
        List<IAggregateResult> Aggregates { get; }
        IQueryable Data { get; }
    }

    public interface IGroupQueryResult : IQueryResult
    {
        string GroupPath { get; set; }
        object GroupValue { get; set; }
    }

    public interface IQueryExecutionResult : IQueryResult
    {
        long TotalRecords { get; set; }
        long? NumberOfPages { get; set; }
    }

   
}
