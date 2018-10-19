using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IIncludeStrategyInterceptor : IQueryInterceptor
    {
        IQueryable InterceptIncludeStrategy(IQueryCriteria criteria, IQueryable queryable);
    }

    public interface IIncludeStrategyInterceptor<T> : IQueryInterceptor
    {
        IQueryable<T> InterceptIncludeStrategy(IQueryCriteria criteria, IQueryable<T> queryable);
    }
}
