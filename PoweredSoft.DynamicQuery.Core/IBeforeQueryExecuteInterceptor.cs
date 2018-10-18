using System.Linq;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IBeforeQueryExecuteInterceptor : IQueryInterceptor
    {
        IQueryable InterceptBeforeQuery(IQueryCriteria criteria, IQueryable queryable);
    }

    public interface IBeforeQueryExecuteInterceptor<T> : IQueryInterceptor
    {
        IQueryable<T> InterceptBeforeQuery(IQueryCriteria criteria, IQueryable<T> queryable);
    }
}
