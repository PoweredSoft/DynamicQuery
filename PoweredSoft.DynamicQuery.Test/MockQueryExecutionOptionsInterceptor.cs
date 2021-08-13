using Microsoft.EntityFrameworkCore.Query;
using PoweredSoft.DynamicQuery.Core;
using System.Linq;

namespace PoweredSoft.DynamicQuery.Test
{
    public partial class GroupInterceptorTests
    {
        public class MockQueryExecutionOptionsInterceptor : IQueryExecutionOptionsInterceptor
        {
            public IQueryExecutionOptions InterceptQueryExecutionOptions(IQueryable queryable, IQueryExecutionOptions current)
            {
                if (queryable.Provider is IAsyncQueryProvider)
                {
                    current.GroupByInMemory = true;
                }

                return current;
            }
        }
    }
}
