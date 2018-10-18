using System.Collections.Generic;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface ISortInteceptor : IQueryInterceptor
    {
        IEnumerable<ISort> InterceptSort(ISort sort);
    }
}
