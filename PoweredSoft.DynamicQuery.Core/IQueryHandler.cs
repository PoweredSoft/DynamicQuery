using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IInterceptableQueryHandler
    {
        void AddInterceptor(IQueryInterceptor interceptor);
        IReadOnlyList<IQueryInterceptor> ResolveInterceptors<TSource>(IQueryCriteria criteria, IQueryable<TSource> queryable);
    }

    public interface IQueryHandler : IInterceptableQueryHandler
    {
        IQueryExecutionResult<TSource> Execute<TSource>(IQueryable<TSource> queryable, IQueryCriteria criteria);
        // IQueryExecutionResult<TRecord> Execute<TSource, TRecord>(IQueryable<TSource> queryable, IQueryCriteria criteria);
        // IQueryExecutionResult<TSource> Execute<TSource>(IQueryable<TSource> queryable, IQueryCriteria criteria, IQueryExecutionOptions options);
        // IQueryExecutionResult<TRecord> Execute<TSource, TRecord>(IQueryable<TSource> queryable, IQueryCriteria criteria, IQueryExecutionOptions options);
    }

    public interface IQueryHandlerAsync : IInterceptableQueryHandler
    {
        Task<IQueryExecutionResult<TSource>> ExecuteAsync<TSource>(IQueryable<TSource> queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default);
        // Task<IQueryExecutionResult<TRecord>> ExecuteAsync<TSource, TRecord>(IQueryable<TSource> queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default);
        // Task<IQueryExecutionResult<TSource>> ExecuteAsync<TSource>(IQueryable<TSource> queryable, IQueryCriteria criteria, IQueryExecutionOptions options, CancellationToken cancellationToken = default);
        // Task<IQueryExecutionResult<TRecord>> ExecuteAsync<TSource, TRecord>(IQueryable<TSource> queryable, IQueryCriteria criteria, IQueryExecutionOptions options, CancellationToken cancellationToken = default);
    }
}
