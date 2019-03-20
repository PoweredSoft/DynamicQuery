using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class GroupTests
    {
        [Fact]
        public void Simple()
        {
            MockContextFactory.SeedAndTestContextFor("GroupTests_Simple", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var shouldResult = ctx.Orders.OrderBy(t => t.Customer).GroupBy(t => t.Customer).Select(t => new
                {
                    Customer = t.Key,
                    Orders = t.ToList() 
                }).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria()
                {
                    Groups = new List<IGroup>
                    {
                        new Group { Path = "Customer" }
                    }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Orders, criteria);

                var data = result.Data.Cast<Order>().ToList();
                // top level should have same amount of group levels.
                Assert.Equal(data.Count, shouldResult.Count);
                for(var i = 0; i < shouldResult.Count; i++)
                {
                    var expected = shouldResult[0];
                    var actual = ((IGroupQueryResult)result.Data[0]);
                    Assert.Equal(expected.Customer.Id, (actual.GroupValue as Customer).Id);

                    var expectedOrderIds = expected.Orders.Select(t => t.Id).ToList();
                    var actualOrderIds = actual.Data.Cast<Order>().Select(t => t.Id).ToList();
                    Assert.Equal(expectedOrderIds, actualOrderIds);
                }
            });
        }

        [Fact]
        public void GroupComplex()
        {
            MockContextFactory.SeedAndTestContextFor("GroupTests_Complex", TestSeeders.SeedTicketScenario, ctx =>
            {
                var criteria = new QueryCriteria()
                {
                    Groups = new List<IGroup>()
                    {
                        new Group { Path = "TicketType" },
                        new Group { Path = "Priority" }
                    },
                    Aggregates = new List<IAggregate>()
                    {
                        new Aggregate { Type = AggregateType.Count }
                    }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Tickets, criteria);

                var firstGroup = result.Data[0] as IGroupQueryResult;
                Assert.NotNull(firstGroup);
                var secondGroup = result.Data[1] as IGroupQueryResult;
                Assert.NotNull(secondGroup);

                var expected = ctx.Tickets.Select(t => t.TicketType).Distinct().Count();
                var c = result.Data.Cast<IGroupQueryResult>().Select(t => t.GroupValue).Count();
                Assert.Equal(expected, c);
            });
        }
    }
}
