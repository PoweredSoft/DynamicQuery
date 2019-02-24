using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PoweredSoft.DynamicLinq.Fluent;
using PoweredSoft.DynamicQuery.Extensions;

namespace PoweredSoft.DynamicQuery
{
    public abstract class QueryHandlerBase : IInterceptableQueryHandler
    {
        protected List<IQueryInterceptor> Interceptors { get; } = new List<IQueryInterceptor>();
        protected IQueryCriteria Criteria { get; set; }
        protected IQueryable QueryableAtStart { get; private set; }
        protected IQueryable CurrentQueryable { get; set; }
        protected Type QueryableUnderlyingType => QueryableAtStart.ElementType;
        protected bool HasGrouping => Criteria.Groups?.Any() == true;
        protected bool HasPaging => Criteria.PageSize.HasValue && Criteria.PageSize > 0;

        protected virtual void Reset(IQueryable queryable, IQueryCriteria criteria)
        {
            Criteria = criteria ?? throw new ArgumentNullException("criteria");
            QueryableAtStart = queryable ?? throw new ArgumentNullException("queryable");
            CurrentQueryable = QueryableAtStart;
        }

        protected virtual void CommonBeforeExecute<T>()
        {
            ApplyIncludeStrategyInterceptors<T>();
            ApplyBeforeFilterInterceptors<T>();
            ApplyFilters<T>();
        }

        public virtual void AddInterceptor(IQueryInterceptor interceptor)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");

            if (!Interceptors.Contains(interceptor))
                Interceptors.Add(interceptor);
        }

        protected virtual IGroup InterceptGroup<T>(IGroup group)
        {
            var ret = Interceptors
                .Where(t => t is IGroupInterceptor)
                .Cast<IGroupInterceptor>()
                .Aggregate(group, (prev, inter) => inter.InterceptGroup(prev));

            return ret;
        }


        protected virtual void ApplyPaging<T>()
        {
            if (!HasPaging)
                return;

            var q = (IQueryable<T>) CurrentQueryable;
            var skip = ((Criteria.Page ?? 1) - 1) * Criteria.PageSize.Value;
            CurrentQueryable = q.Skip(skip).Take(Criteria.PageSize.Value);
        }

        protected virtual void ApplySorting<T>()
        {
            if (Criteria.Sorts?.Any() != true)
            {
                ApplyNoSortInterceptor<T>();
                return;
            }

            bool isAppending = false;
            Criteria.Sorts.ForEach(sort =>
            {
                var transformedSort = InterceptSort<T>(sort);
                if (transformedSort.Count == 0)
                    return;

                transformedSort.ForEach(ts =>
                {
                    CurrentQueryable = CurrentQueryable.OrderBy(ts.Path, ts.Ascending == false ? QueryOrderByDirection.Descending : QueryOrderByDirection.Ascending, isAppending);
                    isAppending = true;
                });
            });
        }

        protected DynamicClass FindMatchingAggregateResult(List<List<DynamicClass>> aggregateResults, List<IGroup> groups, List<IGroupQueryResult> groupResults)
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

        protected virtual IQueryable CreateFetchAggregateSelectExpression<T>(IGroup finalGroup, List<IGroup> previousGroups)
        {
            var groupExpression = CurrentQueryable.GroupBy(QueryableUnderlyingType, gb =>
            {
                var groupKeyIndex = -1;
                previousGroups.ForEach(pg => gb.Path(pg.Path, $"Key_{++groupKeyIndex}"));
                gb.Path(finalGroup.Path, $"Key_{++groupKeyIndex}");
            });

            var selectExpression = groupExpression.Select(sb =>
            {
                var groupKeyIndex = -1;
                previousGroups.ForEach(pg => sb.Key($"Key_{++groupKeyIndex}", $"Key_{groupKeyIndex}"));
                sb.Key($"Key_{++groupKeyIndex}", $"Key_{groupKeyIndex}");
                Criteria.Aggregates.ForEach((a, ai) =>
                {
                    var fa = InterceptAggregate<T>(a);
                    var selectType = ResolveSelectFrom(fa.Type);
                    sb.Aggregate(fa.Path, selectType, $"Agg_{ai}");
                });
            });
            return selectExpression;
        }

        protected virtual List<IAggregateResult> MaterializeCalculateTotalAggregateResult(DynamicClass aggregateResult)
        {
            var ret = new List<IAggregateResult>();
            Criteria.Aggregates.ForEach((a, index) =>
            {
                ret.Add(new AggregateResult()
                {
                    Path = a.Path,
                    Type = a.Type,
                    Value = aggregateResult?.GetDynamicPropertyValue($"Agg_{index}")
                });
            });
            return ret;
        }

        protected virtual IQueryable CreateTotalAggregateSelectExpression<T>(IQueryable queryableAfterFilters)
        {
            var groupExpression = queryableAfterFilters.EmptyGroupBy(QueryableUnderlyingType);
            var selectExpression = groupExpression.Select(sb =>
            {
                Criteria.Aggregates.ForEach((a, index) =>
                {
                    var fa = InterceptAggregate<T>(a);
                    var selectType = ResolveSelectFrom(fa.Type);
                    sb.Aggregate(fa.Path, selectType, $"Agg_{index}");
                });
            });
            return selectExpression;
        }

        protected virtual void CalculatePageCount(IQueryExecutionResult result)
        {
            if (!HasPaging)
                return;

            if (result.TotalRecords < Criteria.PageSize)
                result.NumberOfPages = 1;
            else
                result.NumberOfPages = result.TotalRecords / Criteria.PageSize + (result.TotalRecords % Criteria.PageSize != 0 ? 1 : 0);
        }

        protected virtual IAggregate InterceptAggregate<T>(IAggregate aggregate)
        {
            var ret = Interceptors
                .Where(t => t is IAggregateInterceptor)
                .Cast<IAggregateInterceptor>()
                .Aggregate(aggregate, (prev, inter) => inter.InterceptAggregate(prev));
            return ret;
        }

        protected virtual async Task<List<object>> InterceptConvertTo<T>(List<T> entities)
        {
            await AfterEntityReadInterceptors(entities);

            var objects = entities.Cast<object>().ToList();
            for (var i = 0; i < objects.Count; i++)
                objects[i] = InterceptConvertToObject<T>(objects[i]);

            var pairs = entities.Select((t, index) => Tuple.Create(t, objects[index])).ToList();
            await AfterReadInterceptors<T>(pairs);

            return objects;
        }

        protected virtual async Task AfterEntityReadInterceptors<T>(List<T> entities)
        {
            Interceptors
                .Where(t => t is IAfterReadEntityInterceptor<T>)
                .Cast<IAfterReadEntityInterceptor<T>>()
                .ToList()
                .ForEach(t => t.AfterReadEntity(entities));

            var asyncInterceptors = Interceptors.Where(t => t is IAfterReadEntityInterceptorAsync<T>).Cast<IAfterReadEntityInterceptorAsync<T>>();
            foreach (var interceptor in asyncInterceptors)
                await interceptor.AfterReadEntityAsync(entities);
        }

        protected virtual async Task AfterReadInterceptors<T>(List<Tuple<T, object>> pairs)
        {
            Interceptors
                .Where(t => t is IAfterReadInterceptor<T>)
                .Cast<IAfterReadInterceptor<T>>()
                .ToList()
                .ForEach(t => t.AfterRead(pairs));

            var asyncInterceptors = Interceptors.Where(t => t is IAfterReadInterceptorAsync<T>).Cast<IAfterReadInterceptorAsync<T>>();
            foreach (var interceptor in asyncInterceptors)
                await interceptor.AfterReadAsync(pairs);
        }

        protected virtual object InterceptConvertToObject<T>(object o)
        {
            o = Interceptors
                .Where(t => t is IQueryConvertInterceptor)
                .Cast<IQueryConvertInterceptor>()
                .Aggregate(o, (prev, interceptor) => interceptor.InterceptResultTo(prev));

            o = Interceptors
                .Where(t => t is IQueryConvertInterceptor<T>)
                .Cast<IQueryConvertInterceptor<T>>()
                .Aggregate(o, (prev, interceptor) =>
                {
                    if (prev is T)
                        return interceptor.InterceptResultTo((T)prev);

                    return o;
                });

            return o;
        }

        protected virtual List<ISort> InterceptSort<T>(ISort sort)
        {
            var original = new List<ISort>()
            {
                sort
            };

            var ret = Interceptors
                .Where(t => t is ISortInterceptor)
                .Cast<ISortInterceptor>()
                .Aggregate(original as IEnumerable<ISort>, (prev, inter) => inter.InterceptSort(prev))
                .Distinct();

            return ret.ToList();
        }

        protected virtual void ApplyNoSortInterceptor<T>()
        {
            CurrentQueryable = Interceptors.Where(t => t is INoSortInterceptor)
                .Cast<INoSortInterceptor>()
                .Aggregate(CurrentQueryable, (prev, interceptor) => interceptor.InterceptNoSort(Criteria, prev));

            CurrentQueryable = Interceptors.Where(t => t is INoSortInterceptor<T>)
                .Cast<INoSortInterceptor<T>>()
                .Aggregate((IQueryable<T>)CurrentQueryable, (prev, interceptor) => interceptor.InterceptNoSort(Criteria, prev));
        }


        protected virtual SelectTypes? ResolveSelectFromOrDefault(AggregateType aggregateType) => aggregateType.SelectType();
        protected virtual ConditionOperators? ResolveConditionOperatorFromOrDefault(FilterType filterType) => filterType.ConditionOperator();

        protected virtual ConditionOperators ResolveConditionOperatorFrom(FilterType filterType)
        {
            var ret = ResolveConditionOperatorFromOrDefault(filterType);
            if (ret == null)
                throw new NotSupportedException($"{filterType} is not supported");

            return ret.Value;
        }

        protected virtual SelectTypes ResolveSelectFrom(AggregateType aggregateType)
        {
            var ret = ResolveSelectFromOrDefault(aggregateType);
            if (ret == null)
                throw new NotSupportedException($"{aggregateType} is not supported");

            return ret.Value;
        }

        protected virtual void ApplyFilters<T>()
        {
            if (true != Criteria.Filters?.Any())
                return;

            CurrentQueryable = CurrentQueryable.Query(whereBuilder =>
            {
                Criteria.Filters.ForEach(filter => ApplyFilter<T>(whereBuilder, filter));
            });
        }

        protected virtual void ApplyFilter<T>(WhereBuilder whereBuilder, IFilter filter)
        {
            var transformedFilter = InterceptFilter<T>(filter);
            if (transformedFilter is ISimpleFilter)
                ApplySimpleFilter<T>(whereBuilder, transformedFilter as ISimpleFilter);
            else if (transformedFilter is ICompositeFilter)
                AppleCompositeFilter<T>(whereBuilder, transformedFilter as ICompositeFilter);
            else
                throw new NotSupportedException();
        }

        protected virtual void AppleCompositeFilter<T>(WhereBuilder whereBuilder, ICompositeFilter filter)
        {
            whereBuilder.SubQuery(subWhereBuilder => filter.Filters.ForEach(subFilter => ApplyFilter<T>(subWhereBuilder, subFilter)), filter.And == true);
        }

        protected virtual void ApplySimpleFilter<T>(WhereBuilder whereBuilder, ISimpleFilter filter)
        {
            var resolvedConditionOperator = ResolveConditionOperatorFrom(filter.Type);
            whereBuilder.Compare(filter.Path, resolvedConditionOperator, filter.Value, and: filter.And == true);
        }

        protected virtual IFilter InterceptFilter<T>(IFilter filter)
        {
            var ret = Interceptors.Where(t => t is IFilterInterceptor)
                .Cast<IFilterInterceptor>()
                .Aggregate(filter, (previousFilter, interceptor) => interceptor.InterceptFilter(previousFilter));

            return ret;
        }

        protected virtual void ApplyIncludeStrategyInterceptors<T>()
        {
            CurrentQueryable = Interceptors
                .Where(t => t is IIncludeStrategyInterceptor)
                .Cast<IIncludeStrategyInterceptor>()
                .Aggregate(CurrentQueryable, (prev, interceptor) => interceptor.InterceptIncludeStrategy(Criteria, prev));

            CurrentQueryable = Interceptors
                .Where(t => t is IIncludeStrategyInterceptor<T>)
                .Cast<IIncludeStrategyInterceptor<T>>()
                .Aggregate((IQueryable<T>)CurrentQueryable, (prev, interceptor) => interceptor.InterceptIncludeStrategy(Criteria, prev));
        }

        protected virtual void ApplyBeforeFilterInterceptors<T>()
        {
            CurrentQueryable = Interceptors
                .Where(t => t is IBeforeQueryFilterInterceptor)
                .Cast<IBeforeQueryFilterInterceptor>()
                .Aggregate(CurrentQueryable, (prev, interceptor) => interceptor.InterceptBeforeFiltering(Criteria, prev));

            CurrentQueryable = Interceptors
                .Where(t => t is IBeforeQueryFilterInterceptor<T>)
                .Cast<IBeforeQueryFilterInterceptor<T>>()
                .Aggregate((IQueryable<T>)CurrentQueryable, (prev, interceptor) => interceptor.InterceptBeforeFiltering(Criteria, prev));
        }

        protected virtual List<object> RecursiveRegroup<T>(List<DynamicClass> groupRecords, List<List<DynamicClass>> aggregateResults, IGroup group, List<List<object>> lastLists, List<IGroupQueryResult> parentGroupResults = null)
        {
            var groupIndex = Criteria.Groups.IndexOf(group);
            var isLast = Criteria.Groups.Last() == group;
            var groups = Criteria.Groups.Take(groupIndex + 1).ToList();
            var hasAggregates = Criteria.Aggregates.Any();

            var ret = groupRecords
                .GroupBy(gk => gk.GetDynamicPropertyValue($"Key_{groupIndex}"))
                .Select(t =>
                {
                    var groupResult = new GroupQueryResult();

                    // group results.

                    List<IGroupQueryResult> groupResults;
                    if (parentGroupResults == null)
                        groupResults = new List<IGroupQueryResult> { groupResult };
                    else
                        groupResults = parentGroupResults.Union(new[] { groupResult }).ToList();

                    groupResult.GroupPath = group.Path;
                    groupResult.GroupValue = t.Key;

                    if (hasAggregates)
                    {
                        var matchingAggregate = FindMatchingAggregateResult(aggregateResults, groups, groupResults);
                        if (matchingAggregate == null)
                            Debugger.Break();

                        groupResult.Aggregates = new List<IAggregateResult>();
                        Criteria.Aggregates.ForEach((a, ai) =>
                        {
                            var key = $"Agg_{ai}";
                            var aggregateResult = new AggregateResult
                            {
                                Path = a.Path,
                                Type = a.Type,
                                Value = matchingAggregate.GetDynamicPropertyValue(key)
                            };
                            groupResult.Aggregates.Add(aggregateResult);
                        });
                    }

                    if (isLast)
                    {
                        var entities = t.SelectMany(t2 => t2.GetDynamicPropertyValue<List<T>>("Records")).ToList();
                        groupResult.Data = entities.Cast<object>().ToList();
                        lastLists.Add(groupResult.Data);
                    }
                    else
                    {
                        groupResult.Data = RecursiveRegroup<T>(t.ToList(), aggregateResults, Criteria.Groups[groupIndex + 1], lastLists, groupResults);
                    }

                    return groupResult;
                })
                .AsEnumerable<object>()
                .ToList();
            return ret;
        }

        protected virtual async Task QueryInterceptToGrouped<T>(List<List<object>> lists)
        {
            var entities = lists.SelectMany(t => t).Cast<T>().ToList();
            await AfterEntityReadInterceptors(entities);

            var pairs = new List<Tuple<T, object>>();

            lists.ForEach(innerList =>
            {
                for(var i = 0; i < innerList.Count; i++)
                {
                    var entity = (T)innerList[i];
                    var convertedObject = InterceptConvertToObject<T>(entity);
                    innerList[i] = convertedObject;
                    pairs.Add(Tuple.Create(entity, convertedObject));
                }
            });

            await AfterReadInterceptors<T>(pairs);
        }
    }
}
