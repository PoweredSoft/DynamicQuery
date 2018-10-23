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

        public virtual void AddInterceptor(IQueryInterceptor interceptor)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");

            if (!Interceptors.Contains(interceptor))
                Interceptors.Add(interceptor);
        }

        protected virtual IGroup InterceptGroup<T>(IGroup group)
        {
            var ret = Interceptors
                .Where(t => t is IGroupingInterceptor)
                .Cast<IGroupingInterceptor>()
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

        protected virtual List<object> InterceptConvertTo<T>(List<T> entities)
        {
            var objects = entities.Cast<object>().ToList();
            for (var i = 0; i < objects.Count; i++)
                objects[i] = InterceptConvertToObject<T>(objects[i]);

            return objects;
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
                .Aggregate(CurrentQueryable, (prev, interceptor) => interceptor.InterceptNoSort(prev));

            CurrentQueryable = Interceptors.Where(t => t is INoSortInterceptor<T>)
                .Cast<INoSortInterceptor<T>>()
                .Aggregate((IQueryable<T>)CurrentQueryable, (prev, interceptor) => interceptor.InterceptNoSort(prev));
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
    }
}
