using Microsoft.EntityFrameworkCore;
using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class IncludeStrategyTests
    {
        private class MockIncludeStrategyInterceptor : IIncludeStrategyInterceptor
        {
            public IQueryable InterceptIncludeStrategy(IQueryCriteria criteria, IQueryable queryable)
            {
                queryable = ((IQueryable<Order>)queryable).Include(t => t.Customer);
                return queryable;
            }
        }

        private class MockIncludeStrategyGenericInterceptor : 
            IIncludeStrategyInterceptor<Order>,
            IIncludeStrategyInterceptor<Customer>
        {
            public IQueryable<Order> InterceptIncludeStrategy(IQueryCriteria criteria, IQueryable<Order> queryable)
            {
                return queryable.Include(t => t.Customer);
            }

            public IQueryable<Customer> InterceptIncludeStrategy(IQueryCriteria criteria, IQueryable<Customer> queryable)
            {
                // should not go in here.
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void NonGeneric()
        {
            MockContextFactory.SeedAndTestContextFor("IncludeStrategyTests_NonGeneric", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var interceptor = new MockIncludeStrategyInterceptor();

                // queryable of orders.
                var queryable = ctx.Orders.AsQueryable();

                // pass into the interceptor.
                queryable = (IQueryable<Order>)interceptor.InterceptIncludeStrategy(criteria, queryable);
                
                // query handler should pass by the same interceptor.
                var queryHandler = new QueryHandler(Enumerable.Empty<IQueryInterceptorProvider>());
                queryHandler.AddInterceptor(interceptor);
                var result = queryHandler.Execute(ctx.Orders, criteria);

                // compare results.
                var expected = queryable.ToList();
                Assert.Equal(expected, result.Data.Cast<Order>().ToList());
            });
        }

        [Fact]
        public void Generic()
        {
            MockContextFactory.SeedAndTestContextFor("IncludeStrategyTests_Generic", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var interceptor = new MockIncludeStrategyGenericInterceptor();

                // queryable of orders.
                var queryable = ctx.Orders.AsQueryable();

                // pass into the interceptor.
                queryable = interceptor.InterceptIncludeStrategy(criteria, queryable);

                // query handler should pass by the same interceptor.
                var queryHandler = new QueryHandler(Enumerable.Empty<IQueryInterceptorProvider>());
                queryHandler.AddInterceptor(interceptor);
                var result = queryHandler.Execute(ctx.Orders, criteria);

                // compare results.
                var expected = queryable.ToList();
                Assert.Equal(expected, result.Data.Cast<Order>().ToList());
            });
        }
    }
}
