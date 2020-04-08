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

    public interface IQueryResult<TSource>
    {
        List<IAggregateResult> Aggregates { get; }
        IQueryable<TSource> Data { get; }
    }

    public interface IGroupQueryResult<TRecord> : IQueryResult<TRecord>
    {
        string GroupPath { get; set; }
        object GroupValue { get; set; }
        bool HasSubGroups { get; }
        List<IGroupQueryResult<TRecord>> SubGroups { get; set; }
    }

    public interface IQueryExecutionResultPaging
    {
        long TotalRecords { get; set; }
        long? NumberOfPages { get; set; }
    }

    public interface IQueryExecutionResult<TRecord> : IQueryResult<TRecord>, IQueryExecutionResultPaging
    {

    }

    public interface IQueryExecutionGroupResult<TRecord> : IQueryExecutionResult<TRecord>
    {
        List<IGroupQueryResult<TRecord>> Groups { get; set; }
    }


}
