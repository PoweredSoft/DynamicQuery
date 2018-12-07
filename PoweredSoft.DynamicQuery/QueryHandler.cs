using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using PoweredSoft.DynamicLinq;
using PoweredSoft.DynamicLinq.Fluent;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery
{
    public class QueryHandler : QueryHandlerBase, IQueryHandler
    {
        internal MethodInfo ExecuteGeneric = typeof(QueryHandler).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(t => t.Name == "Execute" && t.IsGenericMethod);
        internal IQueryExecutionResult ExecuteReflected() => (IQueryExecutionResult)ExecuteGeneric.MakeGenericMethod(QueryableUnderlyingType).Invoke(this, new object[] { });

        protected virtual IQueryExecutionResult Execute<T>()
        {
            CommonBeforeExecute<T>();
            return HasGrouping ? ExecuteGrouping<T>() : ExecuteNoGrouping<T>();
        }


        protected virtual IQueryExecutionResult ExecuteGrouping<T>()
        {
            var result = new QueryExecutionResult();

            // preserve queryable.
            var queryableAfterFilters = CurrentQueryable;

            result.TotalRecords = queryableAfterFilters.LongCount();
            CalculatePageCount(result);

            // intercept groups in advance to avoid doing it more than once :)
            var finalGroups = Criteria.Groups.Select(g => InterceptGroup<T>(g)).ToList();

            // get the aggregates.
            var aggregateResults = FetchAggregates<T>(finalGroups);

            // sorting.
            finalGroups.ForEach(fg => Criteria.Sorts.Insert(0, new Sort(fg.Path, fg.Ascending)));

            // apply sorting and paging.
            ApplySorting<T>();
            ApplyPaging<T>();

            // create group & select expression.
            CurrentQueryable = CurrentQueryable.GroupBy(QueryableUnderlyingType, gb => finalGroups.ForEach((fg, index) => gb.Path(fg.Path, $"Key_{index}")));
            CurrentQueryable = CurrentQueryable.Select(sb =>
            {
                finalGroups.ForEach((fg, index) => sb.Key($"Key_{index}", $"Key_{index}"));
                sb.ToList("Records");
            });

            // loop through the grouped records.
            var groupRecords = CurrentQueryable.ToDynamicClassList();

            // now join them into logical collections
            result.Data = RecursiveRegroup<T>(groupRecords, aggregateResults, Criteria.Groups.First());

            result.Aggregates = CalculateTotalAggregate<T>(queryableAfterFilters);
            return result;
        }


        protected virtual List<IAggregateResult> CalculateTotalAggregate<T>(IQueryable queryableAfterFilters)
        {
            if (!Criteria.Aggregates.Any())
                return null;

            IQueryable selectExpression = CreateTotalAggregateSelectExpression<T>(queryableAfterFilters);
            var aggregateResult = selectExpression.ToDynamicClassList().FirstOrDefault();
            return MaterializeCalculateTotalAggregateResult(aggregateResult);
        }
        
        protected virtual List<List<DynamicClass>> FetchAggregates<T>(List<IGroup> finalGroups)
        {
            if (!Criteria.Aggregates.Any())
                return null;
            
            var previousGroups = new List<IGroup>();
            var ret = finalGroups.Select(fg =>
            {
                IQueryable selectExpression = CreateFetchAggregateSelectExpression<T>(fg, previousGroups);
                var aggregateResult = selectExpression.ToDynamicClassList();
                previousGroups.Add(fg);
                return aggregateResult;
            }).ToList();
            return ret;
        }

        protected virtual IQueryExecutionResult ExecuteNoGrouping<T>()
        {
            var result = new QueryExecutionResult();

            // after filter queryable
            var afterFilterQueryable = CurrentQueryable;

            // total records.
            result.TotalRecords = afterFilterQueryable.LongCount();
            CalculatePageCount(result);

            // sorts and paging.
            ApplySorting<T>();
            ApplyPaging<T>();

            // data.
            var entities = ((IQueryable<T>)CurrentQueryable).ToList();
            var records = InterceptConvertTo<T>(entities);
            result.Data = records;

            // aggregates.
            result.Aggregates = CalculateTotalAggregate<T>(afterFilterQueryable);

            return result;
        }
   

        public virtual IQueryExecutionResult Execute(IQueryable queryable, IQueryCriteria criteria)
        {
            Reset(queryable, criteria);
            return ExecuteReflected();
        }
    }
}
