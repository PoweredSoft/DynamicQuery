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

            // preserve queryable.
            var queryableAfterFilters = CurrentQueryable;

            result.TotalRecords = queryableAfterFilters.LongCount();
            CalculatePageCount(result);

            // intercept groups in advance to avoid doing it more than once :)
            var finalGroups = Criteria.Groups.Select(g => InterceptGroup<T>(g)).ToList();

            // get the aggregates.
            var aggregateResults = Criteria.Aggregates.Any() ? FetchAggregates(finalGroups) : null;

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
            result.Data = groupRecords.Select((groupRecord, groupRecordIndex) =>
            {
                var groupRecordResult = new GroupQueryResult();
                List<GroupQueryResult> previousGroupResults = new List<GroupQueryResult>();
                List<IGroup> previousGroups = new List<IGroup>();
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

                    if (previousGroupResults.Any())
                        previousGroupResults.Last().Data.Add(cgrr);

                    previousGroupResults.Add(cgrr);
                    previousGroups.Add(g);

                    // find aggregates for this group.
                    if (Criteria.Aggregates.Any())
                    {
                        var matchingAggregate = FindMatchingAggregateResult(aggregateResults, previousGroups, previousGroupResults);
                        cgrr.Aggregates = new List<IAggregateResult>();
                        Criteria.Aggregates.ForEach((a, ai) =>
                        {
                            var key = $"Agg_{ai}";
                            var aggregateResult = new AggregateResult
                            {
                                Path = a.Path,
                                Type = a.Type,
                                Value = matchingAggregate.GetDynamicPropertyValue(key)
                            };
                            cgrr.Aggregates.Add(aggregateResult);
                        });
                    }
                });

                return (object)groupRecordResult;
            }).ToList();

            result.Aggregates = CalculateTotalAggregate(queryableAfterFilters);
            return result;
        }

        protected virtual List<IAggregateResult> CalculateTotalAggregate(IQueryable queryableAfterFilters)
        {
            if (!Criteria.Aggregates.Any())
                return null;

            var groupExpression = queryableAfterFilters.EmptyGroupBy(QueryableUnderlyingType);
            var selectExpression = groupExpression.Select(sb =>
            {
                Criteria.Aggregates.ForEach((a, index) =>
                {
                    var selectType = ResolveSelectFrom(a.Type);
                    sb.Aggregate(a.Path, selectType, $"Agg_{index}");
                });
            });

            var aggregateResult = selectExpression.ToDynamicClassList().First();
            var ret = new List<IAggregateResult>();
            Criteria.Aggregates.ForEach((a, index) =>
            {
                ret.Add(new AggregateResult()
                {
                    Path = a.Path,
                    Type = a.Type,
                    Value = aggregateResult.GetDynamicPropertyValue($"Agg_{index}")
                });
            });
            return ret;
        }

        private DynamicClass FindMatchingAggregateResult(List<List<DynamicClass>> aggregateResults, List<IGroup> groups, List<GroupQueryResult> groupResults)
        {
            var groupIndex = groupResults.Count - 1;
            var aggregateLevel = aggregateResults[groupIndex];

            var ret = aggregateLevel.FirstOrDefault(al =>
            {
                for (var i = 0; i < groups.Count; i++)
                {
                    if (!al.GetDynamicPropertyValue($"Key_{i}").Equals(groupResults[i].GroupValue))
                        return false;
                }

                return true;
            });
            return ret;
        }


        private List<List<DynamicClass>> FetchAggregates(List<IGroup> finalGroups)
        {
            List<List<DynamicClass>> aggregateResults;
            var previousGroups = new List<IGroup>();
            aggregateResults = finalGroups.Select(fg =>
            {
                var groupExpression = CurrentQueryable.GroupBy(QueryableUnderlyingType, gb =>
                {
                    var groupKeyIndex = -1;
                    previousGroups.ForEach(pg => gb.Path(pg.Path, $"Key_{++groupKeyIndex}"));
                    gb.Path(fg.Path, $"Key_{++groupKeyIndex}");
                });

                var selectExpression = groupExpression.Select(sb =>
                {
                    var groupKeyIndex = -1;
                    previousGroups.ForEach(pg => sb.Key($"Key_{++groupKeyIndex}", $"Key_{groupKeyIndex}"));
                    sb.Key($"Key_{++groupKeyIndex}", $"Key_{groupKeyIndex}");
                    Criteria.Aggregates.ForEach((a, ai) =>
                    {
                        var selectType = ResolveSelectFrom(a.Type);
                        sb.Aggregate(a.Path, selectType, $"Agg_{ai}");
                    });
                });

                var aggregateResult = selectExpression.ToDynamicClassList();
                previousGroups.Add(fg);
                return aggregateResult;
            }).ToList();
            return aggregateResults;
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
            
            // the data.
            result.Data = CurrentQueryable.ToObjectList();
            result.Aggregates = CalculateTotalAggregate(afterFilterQueryable);

            return result;
        }

        public virtual IQueryExecutionResult Execute(IQueryable queryable, IQueryCriteria criteria)
        {
            Reset(queryable, criteria);
            return ExecuteReflected();
        }
    }
}
