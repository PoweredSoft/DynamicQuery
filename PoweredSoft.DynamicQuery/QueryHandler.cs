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
            var result = new GroupedQueryExecutionResult();
            result.TotalRecords = CurrentQueryable.LongCount();

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
                        previousGroups.ForEach(pg =>
                        {
                            var previousGroupCleanedPath = pg.Path.Replace(".", "");
                            gb.Path(pg.Path, $"Key_{previousGroupCleanedPath}");
                        });
                        var cleanedPath = fg.Path.Replace(".", "");
                        gb.Path(fg.Path, $"Key_{cleanedPath}");
                    });

                    var selectExpression = groupExpression.Select(sb =>
                    {
                        previousGroups.ForEach(pg => sb.Key(pg.Path));
                        sb.Key(fg.Path);
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
            CurrentQueryable = CurrentQueryable.GroupBy(QueryableUnderlyingType, gb => finalGroups.ForEach(fg => gb.Path(fg.Path)));
            CurrentQueryable = CurrentQueryable.Select(sb =>
            {
                finalGroups.ForEach(fg => sb.Key(fg.Path));
                sb.ToList("Records");
            });

            var temp = CurrentQueryable.ToDynamicClassList();

            return result;
        }




        protected virtual IQueryExecutionResult ExecuteNoGrouping<T>()
        {
            var result = new QueryExecutionResult();

            // total records.
            result.TotalRecords = CurrentQueryable.LongCount();

            // sorts and paging.
            ApplySorting<T>();
            ApplyPaging<T>();
            
            // the data.
            result.Data = CurrentQueryable.ToObjectList();

            // if there is paging.
            if (HasPaging)
            {
                if (result.TotalRecords < Criteria.PageSize)
                    result.NumberOfPages = 1;
                else
                    result.NumberOfPages = result.TotalRecords / Criteria.PageSize + (result.TotalRecords % Criteria.PageSize != 0 ? 1 : 0);
            }

            return result;
        }

        public virtual IQueryExecutionResult Execute(IQueryable queryable, IQueryCriteria criteria)
        {
            Reset(queryable, criteria);
            return ExecuteReflected();
        }
    }
}
