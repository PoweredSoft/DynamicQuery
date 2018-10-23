using System.Linq;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface INoSortInterceptor : IQueryInterceptor
    {
        IQueryable InterceptNoSort(IQueryCriteria criteria, IQueryable queryable);
    }

    public interface INoSortInterceptor<T> : IQueryInterceptor
    {
        IQueryable<T> InterceptNoSort(IQueryCriteria criteria, IQueryable<T> queryable);
    }
}
