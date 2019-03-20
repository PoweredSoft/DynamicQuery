using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Extensions;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                var groupedResult = result.GroupedResult();

                // top level should have same amount of group levels.
                Assert.Equal(groupedResult.Groups.Count, shouldResult.Count);
                for (var i = 0; i < shouldResult.Count; i++)
                {
                    var expected = shouldResult[0];
                    var actual = groupedResult.Groups[0];
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

                var groupedResult = result.GroupedResult();

                var firstGroup = groupedResult.Groups.FirstOrDefault();
                Assert.NotNull(firstGroup);
                var secondGroup = groupedResult.Groups.Skip(1).FirstOrDefault();
                Assert.NotNull(secondGroup);

                var expected = ctx.Tickets.Select(t => t.TicketType).Distinct().Count();
                var c = groupedResult.Groups.Select(t => t.GroupValue).Count();
                Assert.Equal(expected, c);
            });
        }

        [Fact]
        public void InterceptorsWithGrouping()
        {
            MockContextFactory.SeedAndTestContextFor("GroupTests_InterceptorsWithGrouping", TestSeeders.SeedTicketScenario, ctx =>
            {
                var criteria = new QueryCriteria()
                {
                    Groups = new List<IGroup>()
                    {
                        new Group { Path = "TicketType" }
                    },
                    Aggregates = new List<IAggregate>()
                    {
                        new Aggregate { Type = AggregateType.Count }
                    }
                };

                var interceptor = new InterceptorsWithGrouping();
                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(interceptor);
                var result = queryHandler.Execute<Ticket, InterceptorWithGroupingFakeModel>(ctx.Tickets, criteria);
                Assert.Equal(4, interceptor.Count);
                Assert.True(interceptor.Test);
                Assert.True(interceptor.Test2);
                Assert.True(interceptor.Test3);
                Assert.True(interceptor.Test4);
            });
        }
    }

    class InterceptorWithGroupingFakeModel
    {

    }

    class InterceptorsWithGrouping :
        IAfterReadEntityInterceptor<Ticket>,
        IAfterReadEntityInterceptorAsync<Ticket>,
        IAfterReadInterceptor<Ticket>,
        IAfterReadInterceptorAsync<Ticket>,
        IQueryConvertInterceptor<Ticket>
    {
        public int Count { get; set; } = 0;
        public bool Test { get; set; } = false;
        public bool Test2 { get; set; } = false;
        public bool Test3 { get; set; } = false;
        public bool Test4 { get; set; } = false;

        public void AfterRead(List<Tuple<Ticket, object>> pairs)
        {
            Test = true;
            Count++;
        }

        public async Task AfterReadAsync(List<Tuple<Ticket, object>> pairs, CancellationToken cancellationToken = default(CancellationToken))
        {
            Test2 = true;
            Count++;
        }

        public void AfterReadEntity(List<Ticket> entities)
        {
            Test3 = true;
            Count++;
        }

        public async Task AfterReadEntityAsync(List<Ticket> entities, CancellationToken cancellationToken = default(CancellationToken))
        {
            Test4 = true;
            Count++;
        }

        public object InterceptResultTo(Ticket entity)
        {
            return new InterceptorWithGroupingFakeModel();
        }
    }
}
