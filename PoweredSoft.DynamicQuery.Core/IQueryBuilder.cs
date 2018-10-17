using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IQueryBuilder
    {
        IQueryResult Execute(IQueryable queryable, IQueryCriteria criteria);
        Task<IQueryResult> ExecuteAsync(IQueryable queryable, IQueryCriteria criteria);
        void AddInterceptor(IQueryInterceptor interceptor);
    }

    public interface IQueryInterceptor
    {

    }

    public interface IBeforeQueryAlteredInterceptor : IQueryInterceptor
    {
        IQueryable InterceptQueryBeforeAltered(IQueryCriteria criteria, IQueryable queryable); 
    }

    public interface IBeforeQueryAlteredInterceptor<T> : IQueryInterceptor
    {
        IQueryable<T> InterceptQueryBeforeAltered(IQueryCriteria criteria, IQueryable<T> queryable);
    }

    public interface IFilterInteceptor : IQueryInterceptor
    {
        IEnumerable<IFilter> InterceptFilter(IFilter filter);
    }

    public interface IGroupingInteceptor : IQueryInterceptor
    {
        IGroup InterceptGroup(IGroup group);
    }

    public interface ISortInteceptor : IQueryInterceptor
    {
        IEnumerable<ISort> InterceptSort(ISort sort);
    }

    public interface IBeforeQueryExecuteInterceptor : IQueryInterceptor
    {
        IQueryable InterceptBeforeQuery(IQueryCriteria criteria, IQueryable queryable);
    }

    public interface IBeforeQueryExecuteInterceptor<T> : IQueryInterceptor
    {
        IQueryable<T> InterceptBeforeQuery(IQueryCriteria criteria, IQueryable<T> queryable);
    }
}
