using Microsoft.EntityFrameworkCore;
using PoweredSoft.Data;
using PoweredSoft.Data.EntityFrameworkCore;
using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Extensions;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static PoweredSoft.DynamicQuery.Test.GroupInterceptorTests;

namespace PoweredSoft.DynamicQuery.Test
{
    public class AsyncTests
    {
        [Fact]
        public void TestEmptyCriteria()
        {
            MockContextFactory.SeedAndTestContextFor("AsyncTests_TestEmptyCriteria", TestSeeders.SimpleSeedScenario, async ctx =>
            {
                var resultShouldMatch = ctx.Items.ToList();
                var queryable = ctx.Items.AsQueryable();

                // query handler that is empty should be the same as running to list.
                var aqf = new AsyncQueryableService(new[] { new AsyncQueryableHandlerService() });
                var criteria = new QueryCriteria();
                var queryHandler = new QueryHandlerAsync(aqf, Enumerable.Empty<IQueryInterceptorProvider>());
                var result = await queryHandler.ExecuteAsync(queryable, criteria);
                var data = result.Data.Cast<Item>().ToList();
                Assert.Equal(resultShouldMatch, data);
            });
        }

        /*[Fact]
        public void WithGrouping()
        {
            MockContextFactory.SeedAndTestContextFor("AsyncTests_WithGrouping", TestSeeders.SimpleSeedScenario, async ctx =>
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
                var asyncService = new AsyncQueryableService(new[] { new AsyncQueryableHandlerService() });
                var queryHandler = new QueryHandlerAsync(asyncService, Enumerable.Empty<IQueryInterceptorProvider>());
                var result = await queryHandler.ExecuteAsync(ctx.OrderItems.Include(t => t.Order.Customer), criteria, new QueryExecutionOptions
                {
                    GroupByInMemory = true
                });

                var groups = result.GroupedResult().Groups;

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
        */

        [Fact]
        public void SimpleFilter()
        {
            MockContextFactory.SeedAndTestContextFor("AsyncTests_SimpleFilter", TestSeeders.SimpleSeedScenario, async ctx =>
            {
                var resultShouldMatch = ctx.Items.Where(t => t.Name.EndsWith("Cables")).ToList();

                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter>
                    {
                        new SimpleFilter
                        {
                            Path = "Name",
                            Type = FilterType.EndsWith,
                            Value = "Cables"
                        }
                    }
                };

                var asyncService = new AsyncQueryableService(new[] { new AsyncQueryableHandlerService() });
                var queryHandler = new QueryHandlerAsync(asyncService, Enumerable.Empty<IQueryInterceptorProvider>());
                var result = await queryHandler.ExecuteAsync(ctx.Items, criteria);
                Assert.Equal(resultShouldMatch, result.Data);
            });
        }

        [Fact]
        public void SimpleFilterWithNot()
        {
            MockContextFactory.SeedAndTestContextFor("AsyncTests_SimpleFilter2", TestSeeders.SimpleSeedScenario, async ctx =>
            {
                var resultShouldMatch = ctx.Items.Where(t => !t.Name.EndsWith("Cables")).ToList();

                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter>
                    {
                        new SimpleFilter
                        {
                            Path = "Name",
                            Type = FilterType.EndsWith,
                            Value = "Cables",
                            Not = true
                        }
                    }
                };

                var asyncService = new AsyncQueryableService(new[] { new AsyncQueryableHandlerService() });
                var queryHandler = new QueryHandlerAsync(asyncService, Enumerable.Empty<IQueryInterceptorProvider>());
                var result = await queryHandler.ExecuteAsync(ctx.Items, criteria);
                Assert.Equal(resultShouldMatch, result.Data.Cast<Item>().ToList());
            });
        }

        [Fact]
        public void TestPaging()
        {
            MockContextFactory.SeedAndTestContextFor("AsyncTests_TestPagging", TestSeeders.SimpleSeedScenario, async ctx =>
            {
                var resultShouldMatch = ctx.OrderItems.OrderBy(t => t.Id).Skip(5).Take(5).Where(o=>o.ItemId==0).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria();
                criteria.Sorts.Add(new Sort("Id"));
                criteria.Filters.Add(new SimpleFilter {Path = "ItemId", Value = 0, Type = FilterType.Equal});
                criteria.Page = 2;
                criteria.PageSize = 5;

                var asyncService = new AsyncQueryableService(new[] { new AsyncQueryableHandlerService() });
                var queryHandler = new QueryHandlerAsync(asyncService, Enumerable.Empty<IQueryInterceptorProvider>());
                var result = await queryHandler.ExecuteAsync(ctx.OrderItems, criteria);
                Assert.Equal(resultShouldMatch, result.Data.Cast<OrderItem>().ToList());
            });
        }

        [Fact]
        public void WithGroupingInterceptorOptions()
        {
            MockContextFactory.SeedAndTestContextFor("AsyncTests_WithGroupingInterceptorOptions", TestSeeders.SimpleSeedScenario, async ctx =>
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
                var asyncService = new AsyncQueryableService(new[] { new AsyncQueryableHandlerService() });
                var queryHandler = new QueryHandlerAsync(asyncService, Enumerable.Empty<IQueryInterceptorProvider>());
                queryHandler.AddInterceptor(new MockQueryExecutionOptionsInterceptor());
                var result = await queryHandler.ExecuteAsync(ctx.OrderItems.Include(t => t.Order.Customer), criteria);

                var groups = result.GroupedResult().Groups;

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
