using System;
using System.Collections.Generic;
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
        internal IQueryExecutionResult ExecuteReflected() => (IQueryExecutionResult)ExecuteGeneric.MakeGenericMethod(QueryableUnderlyingType).Invoke(this, new object[]{});

        protected virtual IQueryExecutionResult Execute<T>()
        {
            ApplyIncludeStrategyInterceptors<T>();
            ApplyBeforeFilterInterceptors<T>();
            ApplyFilters<T>();
            return HasGrouping ? ExecuteGrouping<T>() : ExecuteNoGrouping<T>();
        }

        protected virtual IQueryExecutionResult ExecuteGrouping<T>()
        {
            var result = new QueryExecutionResult();
            result.TotalRecords = CurrentQueryable.LongCount();
            CalculatePageCount(result);

            // intercept groups in advance to avoid doing it more than once :)
            var finalGroups = Criteria.Groups.Select(g => InterceptGroup<T>(g)).ToList();

            // get the aggregates.
            List<List<DynamicClass>> aggregateResults = null;
            if (Criteria.Aggregates.Any())
            {
                var previousGroups = new List<IGroup>();
                aggregateResults = finalGroups.Select(fg =>
                {
                    var groupExpression = CurrentQueryable.GroupBy(QueryableUnderlyingType, gb =>
                    {
                        var groupKeyIndex = -1;
                        previousGroups.ForEach(pg =>gb.Path(pg.Path, $"Key_{++groupKeyIndex}"));
                        gb.Path(fg.Path, $"Key_{++groupKeyIndex}");
                    });

                    var selectExpression = groupExpression.Select(sb =>
                    {
                        var groupKeyIndex = -1;
                        previousGroups.ForEach(pg => sb.Key($"Key_{++groupKeyIndex}"));
                        sb.Key($"Key_{++groupKeyIndex}", $"Key_{groupKeyIndex}");
                        Criteria.Aggregates.ForEach(a =>
                        {
                            var selectType = ResolveSelectFrom(a.Type);
                            var pathCleaned = a.Path?.Replace(".", "");
                            sb.Aggregate(a.Path, selectType, $"Agg_{a.Type}_{pathCleaned}");
                        });
                    });

                    var aggregateResult = selectExpression.ToDynamicClassList();
                    previousGroups.Add(fg);
                    return aggregateResult;
                }).ToList();
            }

            // sorting.
            finalGroups.ForEach(fg =>
            {
                Criteria.Sorts.Insert(0, new Sort()
                {
                    Path = fg.Path,
                    Ascending = fg.Ascending
                });
            });

            ApplySorting<T>();
            ApplyPaging<T>();

            // now get the data grouped.
            CurrentQueryable = CurrentQueryable.GroupBy(QueryableUnderlyingType, gb => finalGroups.ForEach((fg, index) => gb.Path(fg.Path, $"Key_{index}")));
            CurrentQueryable = CurrentQueryable.Select(sb =>
            {
                finalGroups.ForEach((fg, index) => sb.Key($"Key_{index}", $"Key_{index}"));
                sb.ToList("Records");
            });

            var groupRecords = CurrentQueryable.ToDynamicClassList();
            result.Data = groupRecords.Select((groupRecord, groupRecordIndex) =>
            {
                var groupRecordResult = new GroupQueryResult();
                GroupQueryResult previous = null;

                Criteria.Groups.ForEach((g, gi) =>
                {
                    bool isFirst = gi == 0;
                    bool isLast = Criteria.Groups.Count - 1 == gi;
                    var cgrr = isFirst ? groupRecordResult : new GroupQueryResult();
                    cgrr.GroupPath = g.Path;
                    cgrr.GroupValue = groupRecord.GetDynamicPropertyValue($"Key_{gi}");


                    if (!isLast)
                        cgrr.Data = new List<object>();
                    else
                        cgrr.Data = groupRecord.GetDynamicPropertyValue<List<T>>("Records").Cast<object>().ToList();

                    if (previous != null)
                        previous.Data.Add(cgrr);

                    previous = cgrr;
                });

                return (object)groupRecordResult;
            }).ToList();

            return result;
        }



        protected virtual IQueryExecutionResult ExecuteNoGrouping<T>()
        {
            var result = new QueryExecutionResult();

            // total records.
            result.TotalRecords = CurrentQueryable.LongCount();
            CalculatePageCount(result);

            // sorts and paging.
            ApplySorting<T>();
            ApplyPaging<T>();
            
            // the data.
            result.Data = CurrentQueryable.ToObjectList();


            return result;
        }

        public virtual IQueryExecutionResult Execute(IQueryable queryable, IQueryCriteria criteria)
        {
            Reset(queryable, criteria);
            return ExecuteReflected();
        }
    }
}
