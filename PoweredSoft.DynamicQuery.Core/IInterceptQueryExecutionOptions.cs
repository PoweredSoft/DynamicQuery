using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IQueryExecutionOptionsInterceptor : IQueryInterceptor
    {
        IQueryExecutionOptions InterceptQueryExecutionOptions(IQueryable queryable, IQueryExecutionOptions current);
    }
}
