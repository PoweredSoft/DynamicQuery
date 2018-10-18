using System.Collections.Generic;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IFilterInterceptor : IQueryInterceptor
    {
        IFilter InterceptFilter(IFilter filter);
    }

    public interface IFilterInterceptor<T> : IQueryInterceptor
    {
        IFilter InterceptFilter<T>(IFilter filter);
    }
}
