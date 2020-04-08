using Microsoft.EntityFrameworkCore;
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
    public class NoSortTests
    {
        private class MockNoSortInterceptor : INoSortInterceptor
        {
            public IQueryable InterceptNoSort(IQueryCriteria criteria, IQueryable queryable)
            {
                return queryable.OrderBy("Customer.LastName");
            }
        }

        private class MockNoSortGenericInterceptor :
            INoSortInterceptor<Order>,
            INoSortInterceptor<Customer>
        {
            public IQueryable<Order> InterceptNoSort(IQueryCriteria criteria, IQueryable<Order> queryable)
            {
                return queryable.OrderBy(t => t.Customer.LastName);
            }

            public IQueryable<Customer> InterceptNoSort(IQueryCriteria criteria, IQueryable<Customer> queryable)
            {
                // should not go in here.
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void NonGeneric()
        {
            MockContextFactory.SeedAndTestContextFor("NoSortTests_NonGeneric", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var interceptor = new MockNoSortInterceptor();

                // queryable of orders.
                var queryable = ctx.Orders.AsQueryable();

                // pass into the interceptor.
                queryable = (IQueryable<Order>)interceptor.InterceptNoSort(criteria, queryable);
                
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
            MockContextFactory.SeedAndTestContextFor("NoSortTests_Generic", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var interceptor = new MockNoSortGenericInterceptor();

                // queryable of orders.
                var queryable = ctx.Orders.AsQueryable();

                // pass into the interceptor.
                queryable = interceptor.InterceptNoSort(criteria, queryable);

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
