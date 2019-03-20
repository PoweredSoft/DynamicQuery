using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class SortInterceptorTests
    {
        private class MockSortInterceptor : ISortInterceptor
        {
            public IEnumerable<ISort> InterceptSort(IEnumerable<ISort> sort)
            {
                return new Sort[]
                {
                    new Sort("Customer.FirstName"),
                    new Sort("Customer.LastName")
                };
            }
        }

        [Fact]
        public void Simple()
        {
            MockContextFactory.SeedAndTestContextFor("SortInterceptorTests_Simple", TestSeeders.SimpleSeedScenario, ctx =>
            {
                // expected
                var expected = ctx.Orders.OrderBy(t => t.Customer.FirstName).ThenBy(t => t.Customer.LastName).ToList();

                // criteria
                var criteria = new QueryCriteria();
                criteria.Sorts.Add(new Sort("CustomerFullName"));
                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(new MockSortInterceptor());
                var result = queryHandler.Execute(ctx.Orders, criteria);
                Assert.Equal(expected, result.Data.Cast<Order>().ToList());
            });
        }
    }
}
