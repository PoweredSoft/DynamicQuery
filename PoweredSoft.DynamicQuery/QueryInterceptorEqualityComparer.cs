using PoweredSoft.DynamicQuery.Core;
using System.Collections.Generic;

namespace PoweredSoft.DynamicQuery
{
    public class QueryInterceptorEqualityComparer : IEqualityComparer<IQueryInterceptor>
    {
        public bool Equals(IQueryInterceptor x, IQueryInterceptor y)
        {
            return x.GetType() == y.GetType();
        }

        public int GetHashCode(IQueryInterceptor obj)
        {
            return obj.GetType().GetHashCode();
        }
    }
}
