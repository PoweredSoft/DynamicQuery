using System.Collections.Generic;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface ISortInterceptor : IQueryInterceptor
    {
        IEnumerable<ISort> InterceptSort(IEnumerable<ISort> sort);
    }
}
