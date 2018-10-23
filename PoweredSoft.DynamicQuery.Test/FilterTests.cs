using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class FilterTests
    {
        [Fact]
        public void SimpleFilter()
        {
            MockContextFactory.SeedAndTestContextFor("FilterTests_SimpleFilter", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var resultShouldMatch = ctx.Items.Where(t => t.Name.EndsWith("Cables")).ToList();

                // query handler that is empty should be the same as running to list.
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

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Items, criteria);
                Assert.Equal(resultShouldMatch, result.Data);
            });
        }

        private class MockIsChuckFilter : ISimpleFilter
        {
            public bool? And { get; set; } = false;
            public FilterType Type { get; set; } = FilterType.Equal;
            public string Path { get; set; } = "FirstName";
            public object Value { get; set; } = "Chuck";
        }

        [Fact]
        public void TestInversionOfControl()
        {
            MockContextFactory.SeedAndTestContextFor("FilterTests_SimpleFilter", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var resultShouldMatch = ctx.Customers.Where(t => t.FirstName == "Chuck").ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter> { new MockIsChuckFilter() }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Customers, criteria);
                Assert.Equal(resultShouldMatch, result.Data);
            });
        }
    }
}
