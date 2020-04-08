using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class QueryProviderTests
    {
        private class FakeInterceptor : IQueryInterceptor
        {

        }

        private class QueryInterceptorProvider : IQueryInterceptorProvider
        {
            public IEnumerable<IQueryInterceptor> GetInterceptors<TSource>(IQueryCriteria queryCriteria, IQueryable<TSource> queryable)
            {
                yield return new FakeInterceptor();
                yield return new FakeInterceptor();
            }
        }

        [Fact]
        public void Simple()
        {
            MockContextFactory.SeedAndTestContextFor("QueryProviderTests_Simple", TestSeeders.SimpleSeedScenario, ctx =>
            {
                // criteria
                var criteria = new QueryCriteria();
                var queryHandler = new QueryHandler(new List<IQueryInterceptorProvider>{
                    new QueryInterceptorProvider()
                });
                queryHandler.AddInterceptor(new FakeInterceptor());
                var interceptors = queryHandler.ResolveInterceptors<Order>(criteria, ctx.Orders);
                Assert.Equal(1, interceptors.Count);
                Assert.True(interceptors[0].GetType() == typeof(FakeInterceptor));
            });
        }
    }
}
