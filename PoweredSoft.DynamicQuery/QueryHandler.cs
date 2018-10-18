using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PoweredSoft.DynamicLinq.Fluent;
using PoweredSoft.DynamicQuery.Extensions;

namespace PoweredSoft.DynamicQuery
{
    public class QueryHandler : IQueryHandler
    {
        protected List<IQueryInterceptor> Interceptors { get; } = new List<IQueryInterceptor>();
        protected IQueryCriteria Criteria { get; set; }
        protected IQueryable QueryableAtStart { get; private set; }
        protected IQueryable CurrentQueryable { get; set; }
        protected Type QueryableUnderlyingType => QueryableAtStart.ElementType;
        private MethodInfo ApplyInterceptorsAndCriteriaMethod { get; } = typeof(QueryHandler).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.Name == "ApplyInterceptorsAndCriteria" && t.IsGenericMethod);

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

        protected virtual void ApplyInterceptorsAndCriteria<T>()
        {
            ApplySimpleBeforeAlterInterceptors();
            ApplyGenericBeforeAlterInterceptors<T>();
            ApplyFilters<T>();
        }

        protected virtual ConditionOperators? ResolveFromOrDefault(FilterType filterType) =>
            filterType.ConditionOperator();

        protected virtual ConditionOperators ResolveFrom(FilterType filterType)
        {
            var ret = ResolveFromOrDefault(filterType);
            if (ret == null)
                throw new NotSupportedException($"{filterType} is not supported");

            return ret.Value;
        }

        protected virtual void ApplyFilters<T>()
        {
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
            var resolvedConditionOperator = ResolveFrom(filter.Type);
            whereBuilder.Compare(filter.Path, resolvedConditionOperator, filter.Value, and: filter.And == true);
        }

        private IFilter InterceptFilter<T>(IFilter filter)
        {
            var ret = Interceptors.Where(t => t is IFilterInterceptor)
                .Cast<IFilterInterceptor>()
                .Aggregate(filter, (previousFilter, interceptor) => interceptor.InterceptFilter(previousFilter));

            return ret;
        }

        private void ApplyInterceptorsAndCriteria()
        {
            var genericMethod = ApplyInterceptorsAndCriteriaMethod.MakeGenericMethod(QueryableUnderlyingType);
            genericMethod.Invoke(this, null);
        }

        protected virtual void ApplyGenericBeforeAlterInterceptors<T>()
        {
            CurrentQueryable = Interceptors
                .Where(t => t is IBeforeQueryAlteredInterceptor<T>)
                .Cast<IBeforeQueryAlteredInterceptor<T>>()
                .Aggregate((IQueryable<T>)CurrentQueryable, (prev, interceptor) => interceptor.InterceptQueryBeforeAltered(Criteria, prev));
        }

        protected virtual void ApplySimpleBeforeAlterInterceptors()
        {
            CurrentQueryable = Interceptors
                .Where(t => t is IBeforeQueryAlteredInterceptor)
                .Cast<IBeforeQueryAlteredInterceptor>()
                .Aggregate(CurrentQueryable, (prev, interceptor) => interceptor.InterceptQueryBeforeAltered(Criteria, prev));
        }

        public virtual IQueryResult Execute(IQueryable queryable, IQueryCriteria criteria)
        {
            Reset(queryable, criteria);
            ApplyInterceptorsAndCriteria();
            var debug = CurrentQueryable.ToObjectList();
            return null;
        }


        public virtual Task<IQueryResult> ExecuteAsync(IQueryable queryable, IQueryCriteria criteria)
        { 
            throw new NotImplementedException();
        }
    }
}
