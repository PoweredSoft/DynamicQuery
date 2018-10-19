using System.Linq;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IBeforeQueryFilterInterceptor : IQueryInterceptor
    {
        IQueryable InterceptBeforeFiltering(IQueryCriteria criteria, IQueryable queryable); 
    }

    public interface IBeforeQueryFilterInterceptor<T> : IQueryInterceptor
    {
        IQueryable<T> InterceptBeforeFiltering(IQueryCriteria criteria, IQueryable<T> queryable);
    }    
}
