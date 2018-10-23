using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using Xunit;
using Xunit.Sdk;

namespace PoweredSoft.DynamicQuery.Test
{
    public class FilterTests
    {
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
            MockContextFactory.SeedAndTestContextFor("FilterTests_TestInversionOfControl", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var resultShouldMatch = ctx.Customers.Where(t => t.FirstName == "Chuck").ToList();

                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter> { new MockIsChuckFilter() }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Customers, criteria);
                Assert.Equal(resultShouldMatch, result.Data);
            });
        }

        [Fact]
        public void SimpleFilter()
        {
            MockContextFactory.SeedAndTestContextFor("FilterTests_SimpleFilter", TestSeeders.SimpleSeedScenario, ctx =>
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

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Items, criteria);
                Assert.Equal(resultShouldMatch, result.Data);
            });
        }



        [Fact]
        public void CompositeFilter()
        {
            MockContextFactory.SeedAndTestContextFor("FilterTests_CompositeFilter", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var resultShouldMatch = ctx.Customers.Where(t => t.FirstName == "John" || t.LastName == "Norris").ToList();

                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter>
                    {
                        new CompositeFilter()
                        {
                            Type = FilterType.Composite,
                            Filters = new List<IFilter>
                            {
                                new SimpleFilter() { Path = "FirstName", Type =  FilterType.Equal, Value = "John" },
                                new SimpleFilter() { Path = "LastName", Type = FilterType.Equal, Value = "Norris"}
                            }
                        }
                    }
                };

                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(ctx.Customers, criteria);
                Assert.Equal(resultShouldMatch, result.Data);
            });
        }
    }
}
