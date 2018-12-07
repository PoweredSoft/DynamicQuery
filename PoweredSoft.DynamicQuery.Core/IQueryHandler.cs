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
        IQueryExecutionResult Execute(IQueryable queryable, IQueryCriteria criteria);
    }

    public interface IQueryHandlerAsync : IInterceptableQueryHandler
    {
        Task<IQueryExecutionResult> ExecuteAsync(IQueryable queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default(CancellationToken));
    }
}
