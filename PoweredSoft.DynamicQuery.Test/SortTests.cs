using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoweredSoft.DynamicLinq;
using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class SortTests
    {
        [Fact]
        public void Simple()
        {
            MockContextFactory.SeedAndTestContextFor("SortTests_Simple", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var shouldResult = ctx.Orders.OrderBy(t => t.OrderNum).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria()
                {
                    Sorts = new List<ISort>()
                    {
                        new Sort("OrderNum")
                    }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Orders, criteria);
                Assert.Equal(shouldResult, result.Data.Cast<Order>().ToList());
            });
        }

        [Fact]
        public void MultiSort()
        {
            MockContextFactory.SeedAndTestContextFor("SortTests_MultiSort", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var shouldResult = ctx.Orders.OrderBy(t => t.Customer.FirstName).ThenBy(t => t.OrderNum).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria()
                {
                    Sorts = new List<ISort>()
                    {
                        new Sort("Customer.FirstName"),
                        new Sort("OrderNum")
                    }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Orders, criteria);
                var data = result.Data.Cast<Order>().ToList();
                Assert.Equal(shouldResult, data);
            });
        }

        private class MockSortInterceptor : ISortInterceptor
        {
            public IEnumerable<ISort> InterceptSort(IEnumerable<ISort> sort)
            {
                if (sort.Count() == 1 && sort.First().Path == "OrderNumStr")
                    return new ISort[] { new Sort("OrderNum", false) };

                return sort;
            }
        }

        [Fact]
        public void SortInterceptor()
        {
            MockContextFactory.SeedAndTestContextFor("SortTests_SortInterceptor", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var shouldResult = ctx.Orders.OrderByDescending(t => t.OrderNum).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria()
                {
                    Sorts = new List<ISort>()
                    {
                        new Sort("OrderNumStr", false)
                    }
                };

                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(new MockSortInterceptor());
                var result = queryHandler.Execute(ctx.Orders, criteria);
                Assert.Equal(shouldResult, result.Data.Cast<Order>().ToList());
            });
        }
    }
}
