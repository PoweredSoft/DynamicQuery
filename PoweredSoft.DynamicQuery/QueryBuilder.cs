using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicQuery
{
    public class QueryBuilder : IQueryBuilder
    {
        protected List<IQueryInterceptor> Interceptors { get; } = new List<IQueryInterceptor>();
        protected IQueryCriteria Criteria { get; set; }
        protected IQueryable QueryableAtStart { get; private set; }
        protected IQueryable CurrentQueryable { get; set; }
        protected Type QueryableUnderlyingType => QueryableAtStart.ElementType;
        private MethodInfo ApplyInterceptorsAndCriteriaMethod { get; } = typeof(QueryBuilder).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.Name == "ApplyInterceptorsAndCriteria" && t.IsGenericMethod);

        protected virtual void Reset(IQueryable queryable, IQueryCriteria criteria)
        {
            //Criteria = criteria ?? throw new ArgumentNullException("criteria");
            QueryableAtStart = queryable ?? throw new ArgumentNullException("queryable");
            CurrentQueryable = QueryableAtStart;
        }

        public virtual void AddInterceptor(IQueryInterceptor interceptor)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");

            if (!Interceptors.Contains(interceptor))
                Interceptors.Add(interceptor);
        }

        protected virtual void ApplyInterceptorsAndCriteria<T>()
        {
            ApplySimpleBeforeAlterInterceptors();
            ApplyGenericBeforeAlterInterceptors<T>();
        }

        private void ApplyInterceptorsAndCriteria()
        {
            var genericMethod = ApplyInterceptorsAndCriteriaMethod.MakeGenericMethod(QueryableUnderlyingType);
            genericMethod.Invoke(this, null);
        }

        protected virtual void ApplyGenericBeforeAlterInterceptors<T>()
        {
            var interceptors = Interceptors.Where(t => t is IBeforeQueryAlteredInterceptor<T>).Cast<IBeforeQueryAlteredInterceptor<T>>().ToList();
            interceptors.ForEach(i => CurrentQueryable = i.InterceptQueryBeforeAltered(Criteria, (IQueryable<T>)CurrentQueryable));
        }

        protected virtual void ApplySimpleBeforeAlterInterceptors()
        {
            var beforeAlterInterceptors = Interceptors.Where(t => t is IBeforeQueryAlteredInterceptor).Cast<IBeforeQueryAlteredInterceptor>().ToList();
            beforeAlterInterceptors.ForEach(i => CurrentQueryable = i.InterceptQueryBeforeAltered(Criteria, CurrentQueryable));
        }

        public virtual IQueryResult Execute(IQueryable queryable, IQueryCriteria criteria)
        {
            Reset(queryable, criteria);
            ApplyInterceptorsAndCriteria();
            return null;
        }


        public virtual Task<IQueryResult> ExecuteAsync(IQueryable queryable, IQueryCriteria criteria)
        { 
            throw new NotImplementedException();
        }
    }
}
