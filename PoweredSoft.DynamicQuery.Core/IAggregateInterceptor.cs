using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IAggregateInterceptor : IQueryInterceptor
    {
        IAggregate InterceptAggregate(IAggregate aggregate);
    }
}
