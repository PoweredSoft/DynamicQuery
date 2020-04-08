using System.Collections.Generic;
using System.Linq;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IQueryInterceptorProvider
    {
        IEnumerable<IQueryInterceptor> GetInterceptors<TSource>(IQueryCriteria queryCriteria, IQueryable<TSource> queryable);
    }
}
