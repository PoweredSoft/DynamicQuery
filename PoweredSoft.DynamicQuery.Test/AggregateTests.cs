using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    
    public class AggregateTests
    {
        [Fact]
        public void WithoutGrouping()
        {
            MockContextFactory.SeedAndTestContextFor("AggregateTests_WithoutGrouping", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var shouldResult = ctx.OrderItems
                    .GroupBy(t => true)
                    .Select(t => new
                    {
                        Count = t.Count(),
                        ItemQuantityAverage = t.Average(t2 => t2.Quantity),
                        ItemQuantitySum = t.Sum(t2 => t2.Quantity),
                        AvgOfPrice = t.Average(t2 => t2.PriceAtTheTime)
                    })
                    .First();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria()
                {
                    Aggregates = new List<Core.IAggregate>
                    {
                        new Aggregate { Type = AggregateType.Count },
                        new Aggregate { Type = AggregateType.Avg, Path = "Quantity" },
                        new Aggregate { Type = AggregateType.Sum, Path = "Quantity" },
                        new Aggregate { Type = AggregateType.Avg, Path = "PriceAtTheTime"}
                    }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.OrderItems, criteria);

                var aggCount = result.Aggregates.First(t => t.Type == AggregateType.Count);
                var aggItemQuantityAverage  = result.Aggregates.First(t => t.Type == AggregateType.Avg && t.Path == "Quantity");
                var aggItemQuantitySum = result.Aggregates.First(t => t.Type == AggregateType.Sum && t.Path == "Quantity");
                var aggAvgOfPrice = result.Aggregates.First(t => t.Type == AggregateType.Avg && t.Path == "PriceAtTheTime");
                Assert.Equal(shouldResult.Count, aggCount.Value);
                Assert.Equal(shouldResult.ItemQuantityAverage, aggItemQuantityAverage.Value);
                Assert.Equal(shouldResult.ItemQuantitySum, aggItemQuantitySum.Value);
                Assert.Equal(shouldResult.AvgOfPrice, aggAvgOfPrice.Value);
            });
        }

        [Fact]
        public void WithGrouping()
        {
            MockContextFactory.SeedAndTestContextFor("AggregateTests_WithGrouping", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var shouldResults = ctx.OrderItems
                    .GroupBy(t => t.Order.CustomerId)
                    .Select(t => new
                    {
                        GroupValue = t.Key,
                        Count = t.Count(),
                        ItemQuantityAverage = t.Average(t2 => t2.Quantity),
                        ItemQuantitySum = t.Sum(t2 => t2.Quantity),
                        AvgOfPrice = t.Average(t2 => t2.PriceAtTheTime)
                    })
                    .ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria()
                {
                    Groups = new List<IGroup>
                    {
                        new Group { Path = "Order.CustomerId" }
                    },
                    Aggregates = new List<Core.IAggregate>
                    {
                        new Aggregate { Type = AggregateType.Count },
                        new Aggregate { Type = AggregateType.Avg, Path = "Quantity" },
                        new Aggregate { Type = AggregateType.Sum, Path = "Quantity" },
                        new Aggregate { Type = AggregateType.Avg, Path = "PriceAtTheTime"}
                    }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.OrderItems, criteria);
                var groups = result.Data.Cast<IGroupQueryResult>().ToList();

                // validate group and aggregates of groups.
                Assert.Equal(groups.Count, shouldResults.Count);
                Assert.All(groups, g =>
                {
                    var index = groups.IndexOf(g);
                    var shouldResult = shouldResults[index];

                    // validate the group value.
                    Assert.Equal(g.GroupValue, shouldResult.GroupValue);

                    // validate the group aggregates.
                    var aggCount = g.Aggregates.First(t => t.Type == AggregateType.Count);
                    var aggItemQuantityAverage = g.Aggregates.First(t => t.Type == AggregateType.Avg && t.Path == "Quantity");
                    var aggItemQuantitySum = g.Aggregates.First(t => t.Type == AggregateType.Sum && t.Path == "Quantity");
                    var aggAvgOfPrice = g.Aggregates.First(t => t.Type == AggregateType.Avg && t.Path == "PriceAtTheTime");
                    Assert.Equal(shouldResult.Count, aggCount.Value);
                    Assert.Equal(shouldResult.ItemQuantityAverage, aggItemQuantityAverage.Value);
                    Assert.Equal(shouldResult.ItemQuantitySum, aggItemQuantitySum.Value);
                    Assert.Equal(shouldResult.AvgOfPrice, aggAvgOfPrice.Value);
                });
            });
        }
    }
}
