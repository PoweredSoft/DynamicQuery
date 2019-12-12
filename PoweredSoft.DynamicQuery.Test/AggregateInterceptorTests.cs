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
    public class AggregateInterceptorTests
    {
        private class MockAggregateInterceptor : IAggregateInterceptor
        {
            public IAggregate InterceptAggregate(IAggregate aggregate) => new Aggregate
            {
                Path = "Price",
                Type = AggregateType.Avg
            };
        }

        [Fact]
        public void Simple()
        {
            MockContextFactory.SeedAndTestContextFor("AggregatorInterceptorTests_Simple", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var expected = ctx.Items
                    .GroupBy(t => true)
                    .Select(t => new
                    {
                        PriceAtTheTime = t.Average(t2 => t2.Price)
                    }).First();

                var criteria = new QueryCriteria();
                criteria.Aggregates.Add(new Aggregate
                {
                    Type = AggregateType.Avg,
                    Path = "ItemPrice"
                });
                var queryHandler = new QueryHandler(Enumerable.Empty<IQueryInterceptorProvider>());
                queryHandler.AddInterceptor(new MockAggregateInterceptor());
                var result = queryHandler.Execute(ctx.Items, criteria);
                Assert.Equal(expected.PriceAtTheTime, result.Aggregates.First().Value);
            });
        }
    }
}
