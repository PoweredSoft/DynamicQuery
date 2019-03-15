using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IInterceptableQueryHandler
    {
        void AddInterceptor(IQueryInterceptor interceptor);
    }

    public interface IQueryHandler : IInterceptableQueryHandler
    {
        IQueryExecutionResult<TRes> Execute<TRes>(IQueryable queryable, IQueryCriteria criteria);
    }

    public interface IQueryHandlerAsync : IInterceptableQueryHandler
    {
        Task<IQueryExecutionResult<TRes>> ExecuteAsync<TRes>(IQueryable queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default(CancellationToken));
    }
}
