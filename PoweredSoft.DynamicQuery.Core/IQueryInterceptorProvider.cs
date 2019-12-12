using System.Collections.Generic;
using System.Linq;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IQueryInterceptorProvider
    {
        IEnumerable<IQueryInterceptor> GetInterceptors<TSource, TResult>(IQueryCriteria queryCriteria, IQueryable<TSource> queryable);
    }
}
