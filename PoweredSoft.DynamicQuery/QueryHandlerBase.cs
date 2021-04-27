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
        private readonly IEnumerable<IQueryInterceptorProvider> queryableInterceptorProviders;

        protected List<IQueryInterceptor> AddedInterceptors { get; } = new List<IQueryInterceptor>();
        protected IQueryCriteria Criteria { get; set; }
        protected IQueryable QueryableAtStart { get; private set; }
        protected IQueryable CurrentQueryable { get; set; }
        protected IQueryExecutionOptions Options { get; private set; }

        protected Type QueryableUnderlyingType => QueryableAtStart.ElementType;
        protected bool HasGrouping => Criteria.Groups?.Any() == true;
        protected bool HasPaging => Criteria.PageSize.HasValue && Criteria.PageSize > 0;

        protected IReadOnlyList<IQueryInterceptor> Interceptors { get; set; }

        protected virtual void ResetInterceptors<TSource, TResult>(IQueryCriteria criteria, IQueryable<TSource> queryable)
        {
            Interceptors = ResolveInterceptors<TSource, TResult>(criteria, queryable);
        }

        public QueryHandlerBase(IEnumerable<IQueryInterceptorProvider> queryableInterceptorProviders)
        {
            this.queryableInterceptorProviders = queryableInterceptorProviders;
        }

        protected virtual void Reset<TSource, TResult>(IQueryable<TSource> queryable, IQueryCriteria criteria, IQueryExecutionOptions options)
        {
            ResetInterceptors<TSource, TResult>(criteria, queryable);
            Criteria = criteria ?? throw new ArgumentNullException("criteria");
            QueryableAtStart = queryable ?? throw new ArgumentNullException("queryable");
            CurrentQueryable = QueryableAtStart;
            Options = options;
        }

        protected virtual void CommonBeforeExecute<TSource>()
        {
            ApplyQueryExecutionOptionIncerceptors();
            ApplyIncludeStrategyInterceptors<TSource>();
            ApplyBeforeFilterInterceptors<TSource>();
            ApplyFilters<TSource>();
        }

        protected virtual void ApplyQueryExecutionOptionIncerceptors()
        {
            Options = Interceptors
                .Where(t => t is IQueryExecutionOptionsInterceptor)
                .Cast<IQueryExecutionOptionsInterceptor>()
                .Aggregate(Options, (prev, curr) => curr.InterceptQueryExecutionOptions(CurrentQueryable, prev));
        }

        public virtual void AddInterceptor(IQueryInterceptor interceptor)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");

            if (!AddedInterceptors.Contains(interceptor))
                AddedInterceptors.Add(interceptor);
        }

        protected virtual IGroup InterceptGroup<TSource>(IGroup group)
        {
            var ret = Interceptors
                .Where(t => t is IGroupInterceptor)
                .Cast<IGroupInterceptor>()
                .Aggregate(group, (prev, inter) => inter.InterceptGroup(prev));

            return ret;
        }


        protected virtual void ApplyPaging<TSource>()
        {
            if (!HasPaging)
                return;

            var q = (IQueryable<TSource>) CurrentQueryable;
            var skip = ((Criteria.Page ?? 1) - 1) * Criteria.PageSize.Value;
            CurrentQueryable = q.Skip(skip).Take(Criteria.PageSize.Value);
        }

        protected virtual void ApplySorting<TSource>()
        {
            if (Criteria.Sorts?.Any() != true)
            {
                ApplyNoSortInterceptor<TSource>();
                return;
            }

            bool isAppending = false;
            Criteria.Sorts.ForEach(sort =>
            {
                var transformedSort = InterceptSort<TSource>(sort);
                if (transformedSort.Count == 0)
                    return;

                transformedSort.ForEach(ts =>
                {
                    CurrentQueryable = CurrentQueryable.OrderBy(ts.Path, ts.Ascending == false ? QueryOrderByDirection.Descending : QueryOrderByDirection.Ascending, isAppending);
                    isAppending = true;
                });
            });
        }

        protected DynamicClass FindMatchingAggregateResult<TRecord>(List<List<DynamicClass>> aggregateResults, List<IGroup> groups, List<IGroupQueryResult<TRecord>> groupResults)
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

        protected virtual IQueryable CreateFetchAggregateSelectExpression<TSource>(IGroup finalGroup, List<IGroup> previousGroups)
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
                    var fa = InterceptAggregate<TSource>(a);
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

        protected virtual IQueryable CreateTotalAggregateSelectExpression<TSource>(IQueryable queryableAfterFilters)
        {
            var groupExpression = queryableAfterFilters.EmptyGroupBy(QueryableUnderlyingType);
            var selectExpression = groupExpression.Select(sb =>
            {
                Criteria.Aggregates.ForEach((a, index) =>
                {
                    var fa = InterceptAggregate<TSource>(a);
                    var selectType = ResolveSelectFrom(fa.Type);
                    sb.Aggregate(fa.Path, selectType, $"Agg_{index}");
                });
            });
            return selectExpression;
        }

        protected virtual void CalculatePageCount(IQueryExecutionResultPaging result)
        {
            if (!HasPaging)
                return;

            if (result.TotalRecords < Criteria.PageSize)
                result.NumberOfPages = 1;
            else
                result.NumberOfPages = result.TotalRecords / Criteria.PageSize + (result.TotalRecords % Criteria.PageSize != 0 ? 1 : 0);
        }

        protected virtual IAggregate InterceptAggregate<TSource>(IAggregate aggregate)
        {
            var ret = Interceptors
                .Where(t => t is IAggregateInterceptor)
                .Cast<IAggregateInterceptor>()
                .Aggregate(aggregate, (prev, inter) => inter.InterceptAggregate(prev));
            return ret;
        }

        protected virtual async Task<List<TRecord>> InterceptConvertTo<TSource, TRecord>(List<TSource> entities)
        {
            await AfterEntityReadInterceptors(entities);

            var ret = new List<TRecord>();
            for (var i = 0; i < entities.Count; i++)
                ret.Add(InterceptConvertToObject<TSource, TRecord>(entities[i]));

            var pairs = entities.Select((t, index) => Tuple.Create(t, ret[index])).ToList();
            await AfterReadInterceptors<TSource, TRecord>(pairs);

            return ret;
        }

        protected virtual async Task AfterEntityReadInterceptors<TSource>(List<TSource> entities)
        {
            Interceptors
                .Where(t => t is IAfterReadEntityInterceptor<TSource>)
                .Cast<IAfterReadEntityInterceptor<TSource>>()
                .ToList()
                .ForEach(t => t.AfterReadEntity(entities));

            var asyncInterceptors = Interceptors.Where(t => t is IAfterReadEntityInterceptorAsync<TSource>).Cast<IAfterReadEntityInterceptorAsync<TSource>>();
            foreach (var interceptor in asyncInterceptors)
                await interceptor.AfterReadEntityAsync(entities);
        }

        protected virtual async Task AfterReadInterceptors<TSource, TRecord>(List<Tuple<TSource, TRecord>> pairs)
        {
            var objPair = pairs.Select(t => Tuple.Create(t.Item1, (object)t.Item2)).ToList();

            Interceptors
                .Where(t => t is IAfterReadInterceptor<TSource>)
                .Cast<IAfterReadInterceptor<TSource>>()
                .ToList()
                .ForEach(t => t.AfterRead(objPair));

            var asyncInterceptors = Interceptors.Where(t => t is IAfterReadInterceptorAsync<TSource>).Cast<IAfterReadInterceptorAsync<TSource>>();
            foreach (var interceptor in asyncInterceptors)
                await interceptor.AfterReadAsync(objPair);

            var asyncInterceptors2 = Interceptors.Where(t => t is IAfterReadInterceptorAsync<TSource, TRecord>).Cast<IAfterReadInterceptorAsync<TSource, TRecord>>();
            foreach (var interceptor in asyncInterceptors2)
                await interceptor.AfterReadAsync(pairs);
        }

        protected virtual TRecord InterceptConvertToObject<TSource, TRecord>(object o)
        {
            o = Interceptors
                .Where(t => t is IQueryConvertInterceptor)
                .Cast<IQueryConvertInterceptor>()
                .Aggregate(o, (prev, interceptor) => interceptor.InterceptResultTo(prev));

            o = Interceptors
                .Where(t => t is IQueryConvertInterceptor<TSource>)
                .Cast<IQueryConvertInterceptor<TSource>>()
                .Aggregate(o, (prev, interceptor) =>
                {
                    if (prev is TSource)
                        return interceptor.InterceptResultTo((TSource)prev);

                    return o;
                });

            o = Interceptors
               .Where(t => t is IQueryConvertInterceptor<TSource, TRecord>)
               .Cast<IQueryConvertInterceptor<TSource, TRecord>>()
               .Aggregate(o, (prev, interceptor) =>
               {
                   if (prev is TSource)
                       return interceptor.InterceptResultTo((TSource)prev);

                   return o;
               });

            return (TRecord)o;
        }

        protected virtual List<ISort> InterceptSort<TSource>(ISort sort)
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

        protected virtual void ApplyNoSortInterceptor<TSource>()
        {
            CurrentQueryable = Interceptors.Where(t => t is INoSortInterceptor)
                .Cast<INoSortInterceptor>()
                .Aggregate(CurrentQueryable, (prev, interceptor) => interceptor.InterceptNoSort(Criteria, prev));

            CurrentQueryable = Interceptors.Where(t => t is INoSortInterceptor<TSource>)
                .Cast<INoSortInterceptor<TSource>>()
                .Aggregate((IQueryable<TSource>)CurrentQueryable, (prev, interceptor) => interceptor.InterceptNoSort(Criteria, prev));
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

        protected virtual void ApplyFilters<TSource>()
        {
            if (true != Criteria.Filters?.Any())
                return;

            CurrentQueryable = CurrentQueryable.Query(whereBuilder =>
            {
                Criteria.Filters.ForEach(filter => ApplyFilter<TSource>(whereBuilder, filter));
            });
        }

        protected virtual void ApplyFilter<TSource>(WhereBuilder whereBuilder, IFilter filter)
        {
            var transformedFilter = InterceptFilter<TSource>(filter);
            if (transformedFilter is ISimpleFilter)
                ApplySimpleFilter<TSource>(whereBuilder, transformedFilter as ISimpleFilter);
            else if (transformedFilter is ICompositeFilter)
                AppleCompositeFilter<TSource>(whereBuilder, transformedFilter as ICompositeFilter);
            else
                throw new NotSupportedException();
        }

        protected virtual void AppleCompositeFilter<TSource>(WhereBuilder whereBuilder, ICompositeFilter filter)
        {
            whereBuilder.SubQuery(subWhereBuilder => filter.Filters.ForEach(subFilter => ApplyFilter<TSource>(subWhereBuilder, subFilter)), filter.And == true);
        }

        protected virtual void ApplySimpleFilter<TSource>(WhereBuilder whereBuilder, ISimpleFilter filter)
        {
            var resolvedConditionOperator = ResolveConditionOperatorFrom(filter.Type);

            if (filter.CaseInsensitive == true)
            {
                filter.Path += ".ToLower()";
                filter.Value = $"{filter.Value}"?.ToLower();
            }

            whereBuilder.Compare(filter.Path, resolvedConditionOperator, filter.Value, and: filter.And == true, negate: filter.Not == true);
        }

        protected virtual IFilter InterceptFilter<TSource>(IFilter filter)
        {
            var ret = Interceptors.Where(t => t is IFilterInterceptor)
                .Cast<IFilterInterceptor>()
                .Aggregate(filter, (previousFilter, interceptor) => interceptor.InterceptFilter(previousFilter));

            return ret;
        }

        protected virtual void ApplyIncludeStrategyInterceptors<TSource>()
        {
            CurrentQueryable = Interceptors
                .Where(t => t is IIncludeStrategyInterceptor)
                .Cast<IIncludeStrategyInterceptor>()
                .Aggregate(CurrentQueryable, (prev, interceptor) => interceptor.InterceptIncludeStrategy(Criteria, prev));

            CurrentQueryable = Interceptors
                .Where(t => t is IIncludeStrategyInterceptor<TSource>)
                .Cast<IIncludeStrategyInterceptor<TSource>>()
                .Aggregate((IQueryable<TSource>)CurrentQueryable, (prev, interceptor) => interceptor.InterceptIncludeStrategy(Criteria, prev));
        }

        protected virtual void ApplyBeforeFilterInterceptors<TSource>()
        {
            CurrentQueryable = Interceptors
                .Where(t => t is IBeforeQueryFilterInterceptor)
                .Cast<IBeforeQueryFilterInterceptor>()
                .Aggregate(CurrentQueryable, (prev, interceptor) => interceptor.InterceptBeforeFiltering(Criteria, prev));

            CurrentQueryable = Interceptors
                .Where(t => t is IBeforeQueryFilterInterceptor<TSource>)
                .Cast<IBeforeQueryFilterInterceptor<TSource>>()
                .Aggregate((IQueryable<TSource>)CurrentQueryable, (prev, interceptor) => interceptor.InterceptBeforeFiltering(Criteria, prev));
        }

        protected virtual List<IGroupQueryResult<TRecord>> RecursiveRegroup<TSource, TRecord>(List<DynamicClass> groupRecords, List<List<DynamicClass>> aggregateResults, IGroup group, List<(List<TSource> entities, IGroupQueryResult<TRecord> group)> lastLists, List<IGroupQueryResult<TRecord>> parentGroupResults = null)
        {
            var groupIndex = Criteria.Groups.IndexOf(group);
            var isLast = Criteria.Groups.Last() == group;
            var groups = Criteria.Groups.Take(groupIndex + 1).ToList();
            var hasAggregates = Criteria.Aggregates.Any();

            var ret = groupRecords
                .GroupBy(gk => gk.GetDynamicPropertyValue($"Key_{groupIndex}"))
                .Select(t =>
                {
                    var groupResult = new GroupQueryResult<TRecord>();

                    // group results.

                    List<IGroupQueryResult<TRecord>> groupResults;
                    if (parentGroupResults == null)
                        groupResults = new List<IGroupQueryResult<TRecord>> { groupResult };
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
                        var entities = t.SelectMany(t2 => t2.GetDynamicPropertyValue<List<TSource>>("Records")).ToList();
                        var tuple = (entities, groupResult);
                        groupResult.Data = new List<TRecord>();
                        lastLists.Add(tuple);
                    }
                    else
                    {
                        groupResult.SubGroups = RecursiveRegroup<TSource, TRecord>(t.ToList(), aggregateResults, Criteria.Groups[groupIndex + 1], lastLists, groupResults);
                    }

                    return groupResult;
                })
                .AsEnumerable<IGroupQueryResult<TRecord>>()
                .ToList();

            return ret;
        }

        protected virtual async Task QueryInterceptToGrouped<TSource, TRecord>(List<(List<TSource> entities, IGroupQueryResult<TRecord> group)> lists)
        {
            var entities = lists.SelectMany(t => t.entities).ToList();
            await AfterEntityReadInterceptors(entities);

            var pairs = new List<Tuple<TSource, TRecord>>();

            lists.ForEach(innerList =>
            {
                for (var i = 0; i < innerList.entities.Count; i++)
                {
                    var entity = innerList.entities[i];
                    var convertedObject = InterceptConvertToObject<TSource, TRecord>(entity);
                    innerList.group.Data.Add(convertedObject);
                    pairs.Add(Tuple.Create(entity, convertedObject));
                }
            });

            await AfterReadInterceptors<TSource, TRecord>(pairs);
        }

        public IReadOnlyList<IQueryInterceptor> ResolveInterceptors<TSource, TResult>(IQueryCriteria criteria, IQueryable<TSource> queryable)
        {
            var providedInterceptors = queryableInterceptorProviders.SelectMany(t => t.GetInterceptors<TSource, TResult>(criteria, queryable)).ToList();
            var final = providedInterceptors
                .Concat(AddedInterceptors)
                .Distinct(new QueryInterceptorEqualityComparer())
                .ToList();

            return final;
        }
    }
}
