using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IAfterReadEntityInterceptor<T> : IQueryInterceptor
    {
        void AfterReadEntity(List<T> entities);
    }

    public interface IAfterReadEntityInterceptorAsync<T> : IQueryInterceptor
    {
        Task AfterReadEntityAsync(List<T> entities, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IAfterReadInterceptor<T> : IQueryInterceptor
    {
        void AfterRead(List<Tuple<T, object>> pairs);
    }

    public interface IAfterReadInterceptorAsync<T> : IQueryInterceptor
    {
        Task AfterReadAsync(List<Tuple<T, object>> pairs, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IAfterReadInterceptor<T, T2> : IQueryInterceptor
    {
        void AfterRead(List<Tuple<T, T2>> pairs);
    }

    public interface IAfterReadInterceptorAsync<T, T2> : IQueryInterceptor
    {
        Task AfterReadAsync(List<Tuple<T, T2>> pairs, CancellationToken cancellationToken = default(CancellationToken));
    }
}
