using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using PoweredSoft.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class BeforeFilterTests
    {
        private class MockBeforeQueryFilterInterceptor : IBeforeQueryFilterInterceptor
        {
            public IQueryable InterceptBeforeFiltering(IQueryCriteria criteria, IQueryable queryable)
            {
                return queryable.Where(t => t.Equal("Customer.FirstName", "David"));
            }
        }

        private class MockBeforeQueryFilterGenericInterceptor :
            IBeforeQueryFilterInterceptor<Order>,
            IBeforeQueryFilterInterceptor<Customer>
        {
            public IQueryable<Order> InterceptBeforeFiltering(IQueryCriteria criteria, IQueryable<Order> queryable)
            {
                return queryable.Where(t => t.Customer.FirstName == "David");
            }

            public IQueryable<Customer> InterceptBeforeFiltering(IQueryCriteria criteria, IQueryable<Customer> queryable)
            {
                // leave throw it validates the test, if it gets in here it shoulnd't
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void NonGeneric()
        {
            MockContextFactory.SeedAndTestContextFor("BeforeFilterTests_NonGeneric", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var interceptor = new MockBeforeQueryFilterInterceptor();

                // queryable of orders.
                var queryable = ctx.Orders.AsQueryable();

                // pass into the interceptor.
                queryable = (IQueryable<Order>)interceptor.InterceptBeforeFiltering(criteria, queryable);

                // query handler should pass by the same interceptor.
                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(interceptor);
                var result = queryHandler.Execute(ctx.Orders, criteria);

                // compare results.
                var expected = queryable.ToList();
                Assert.Equal(expected, result.Data);
            });
        }

        [Fact]
        public void Generic()
        {
            MockContextFactory.SeedAndTestContextFor("BeforeFilterTests_Generic", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var interceptor = new MockBeforeQueryFilterGenericInterceptor();

                // queryable of orders.
                var queryable = ctx.Orders.AsQueryable();

                // pass into the interceptor.
                queryable = interceptor.InterceptBeforeFiltering(criteria, queryable);

                // query handler should pass by the same interceptor.
                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(interceptor);
                var result = queryHandler.Execute(ctx.Orders, criteria);

                // compare results.
                var expected = queryable.ToList();
                Assert.Equal(expected, result.Data);
            });
        }
    }
}
