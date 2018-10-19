using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using PoweredSoft.DynamicLinq;
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
            throw new NotImplementedException();
        }

        protected virtual IQueryExecutionResult ExecuteNoGrouping<T>()
        {
            var result = new QueryExecutionResult();

            // total records.
            result.TotalRecords = CurrentQueryable.LongCount();

            // sorts and paging.
            ApplyNoGroupingSorts<T>();
            ApplyNoGroupingPaging<T>();
            
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
