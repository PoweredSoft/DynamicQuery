using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class GroupInterceptorTests
    {
        private class MockGroupInterceptor : IGroupInterceptor
        {
            public IGroup InterceptGroup(IGroup group)
            {
                return new Group()
                {
                    Path = "Customer.FirstName"
                };
            }
        }

        [Fact]
        public void Simple()
        {
            MockContextFactory.SeedAndTestContextFor("GroupInterceptorTests_Simple", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var expected = ctx.Orders
                    .OrderBy(t => t.Customer.FirstName)
                    .GroupBy(t => t.Customer.FirstName)
                    .Select(t => t.Key)
                    .ToList();

                var criteria = new QueryCriteria();
                criteria.Groups.Add(new Group { Path = "CustomerFirstName" });
                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(new MockGroupInterceptor());
                var result = queryHandler.Execute(ctx.Orders, criteria);
                var actual = result.Data.Cast<IGroupQueryResult>().Select(t => t.GroupValue).ToList();
                Assert.Equal(expected, actual);
            });
        }
    }
}
