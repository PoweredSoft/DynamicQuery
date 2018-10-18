using System.Collections.Generic;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IFilterInterceptor : IQueryInterceptor
    {
        IFilter InterceptFilter(IFilter filter);
    }
}
