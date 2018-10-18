using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicQuery.Core
{
    public interface IQueryHandler
    {
        IQueryResult Execute(IQueryable queryable, IQueryCriteria criteria);
        Task<IQueryResult> ExecuteAsync(IQueryable queryable, IQueryCriteria criteria);
        void AddInterceptor(IQueryInterceptor interceptor);
    }
}
