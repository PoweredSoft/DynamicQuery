using System.Linq;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IBeforeQueryAlteredInterceptor : IQueryInterceptor
    {
        IQueryable InterceptQueryBeforeAltered(IQueryCriteria criteria, IQueryable queryable); 
    }

    public interface IBeforeQueryAlteredInterceptor<T> : IQueryInterceptor
    {
        IQueryable<T> InterceptQueryBeforeAltered(IQueryCriteria criteria, IQueryable<T> queryable);
    }    
}
