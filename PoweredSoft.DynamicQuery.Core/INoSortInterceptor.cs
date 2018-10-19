using System.Linq;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface INoSortInterceptor : IQueryInterceptor
    {
        IQueryable InterceptNoSort(IQueryable queryable);
    }

    public interface INoSortInterceptor<T> : IQueryInterceptor
    {
        IQueryable<T> InterceptNoSort(IQueryable<T> queryable);
    }
}
