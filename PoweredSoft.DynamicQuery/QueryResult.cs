using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery
{
    /// <summary>
    /// Represents an aggregate result.
    /// </summary>
    public class AggregateResult : IAggregateResult
    {
        public string Path { get; set; }
        public AggregateType Type { get; set; }
        public object Value { get; set; }

        public bool ShouldSerializePath() => !string.IsNullOrWhiteSpace(Path);
    }

    // part of a result.
    public abstract class QueryResult<TRecord> : IQueryResult<TRecord>
    {
        public List<IAggregateResult> Aggregates { get; set; }
        public IQueryable<TRecord> Data { get; set; }

        public bool ShouldSerializeAggregates() => Aggregates != null;

        public bool ShouldSerializeData() => Data != null;
    }

    // just result
    public class QueryExecutionResult<TRecord> : QueryResult<TRecord>, IQueryExecutionResult<TRecord>
    {
        public long TotalRecords { get; set; }
        public long? NumberOfPages { get; set; }
        
        public bool ShouldSerializeNumberOfPages() => NumberOfPages.HasValue;
    }

    public class QueryExecutionGroupResult<TRecord> : QueryExecutionResult<TRecord>, IQueryExecutionGroupResult<TRecord>
    {
        public List<IGroupQueryResult<TRecord>> Groups { get; set; }
    }

    // group result.
    public class GroupQueryResult<TRecord> : QueryResult<TRecord>, IGroupQueryResult<TRecord>
    {
        public string GroupPath { get; set; }
        public object GroupValue { get; set; }
        public bool HasSubGroups => SubGroups != null && SubGroups.Count >= 1;
        public List<IGroupQueryResult<TRecord>> SubGroups { get; set; }

        public bool ShouldSerializeSubGroups() => HasSubGroups;
    }
}
