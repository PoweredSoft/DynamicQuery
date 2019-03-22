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
        IQueryExecutionResult<TSource> Execute<TSource>(IQueryable<TSource> queryable, IQueryCriteria criteria);
        IQueryExecutionResult<TRecord> Execute<TSource, TRecord>(IQueryable<TSource> queryable, IQueryCriteria criteria);
    }

    public interface IQueryHandlerAsync : IInterceptableQueryHandler
    {
        Task<IQueryExecutionResult<TSource>> ExecuteAsync<TSource>(IQueryable<TSource> queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default(CancellationToken));
        Task<IQueryExecutionResult<TRecord>> ExecuteAsync<TSource, TRecord>(IQueryable<TSource> queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default(CancellationToken));
    }
}
