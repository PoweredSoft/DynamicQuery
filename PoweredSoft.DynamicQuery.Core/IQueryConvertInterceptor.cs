using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IQueryConvertInterceptor : IQueryInterceptor
    {
        object InterceptResultTo(object entity);
    }

    public interface IQueryConvertInterceptor<T> : IQueryInterceptor
    {
        object InterceptResultTo(T entity);
    }
}
