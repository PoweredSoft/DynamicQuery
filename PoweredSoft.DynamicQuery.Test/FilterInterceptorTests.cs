using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Extensions;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class FilterInterceptorTests
    {
        private class MockFilterInterceptorA : IFilterInterceptor
        {
            public IFilter InterceptFilter(IFilter filter)
            {
                if (filter is ISimpleFilter && ((ISimpleFilter)filter).Path == "CustomerFirstName")
                    return new SimpleFilter { Path = "Customer.FirstName", Type = FilterType.Contains, Value = "David" };

                return filter;
            }
        }

        private class MockFilterInterceptorAWithExtension : IFilterInterceptor
        {
            public IFilter InterceptFilter(IFilter filter)
            {
                if (filter.IsSimpleFilterOn("CustomerFirstName"))
                    return filter.ReplaceByOn<Order>(t => t.Customer.FirstName);
                else if (filter.IsSimpleFilterOn("CustomerFullName"))
                    return filter.ReplaceByCompositeOn<Order>(t => t.Customer.FirstName, t => t.Customer.LastName);

                return filter;
            }
        }

        private class MockFilterInterceptorB : IFilterInterceptor
        {
            public IFilter InterceptFilter(IFilter filter)
            {
                return new SimpleFilter { Path = "Customer.LastName", Type = FilterType.Contains, Value = "Norris" };
            }
        }

        [Fact]
        public void Simple()
        {
            MockContextFactory.SeedAndTestContextFor("FilterInterceptorTests_Simple", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var queryable = ctx.Orders.AsQueryable();
                
                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter>
                    {
                        new SimpleFilter { Path = "CustomerFirstName", Value = "David", Type = FilterType.Contains }
                    }
                };

                var query = new QueryHandler();
                query.AddInterceptor(new MockFilterInterceptorA());
                var result = query.Execute(queryable, criteria);

                var actual = result.Data;
                var expected = queryable.Where(t => t.Customer.FirstName == "David").ToList();
                Assert.Equal(expected, actual);
            });
        }

        [Fact]
        public void SimpleWithExtensions()
        {
            MockContextFactory.SeedAndTestContextFor("FilterInterceptorTests_SimpleWithExtensions", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var queryable = ctx.Orders.AsQueryable();

                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter>
                    {
                        new SimpleFilter { Path = "CustomerFirstName", Value = "David", Type = FilterType.Contains }
                    }
                };

                var query = new QueryHandler();
                query.AddInterceptor(new MockFilterInterceptorAWithExtension());
                var result = query.Execute(queryable, criteria);

                var actual = result.Data;
                var expected = queryable.Where(t => t.Customer.FirstName == "David").ToList();
                Assert.Equal(expected, actual);
            });
        }

        [Fact]
        public void SimpleWithExtensions2()
        {
            MockContextFactory.SeedAndTestContextFor("FilterInterceptorTests_SimpleWithExtensions2", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var queryable = ctx.Orders.AsQueryable();

                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter>
                    {
                        new SimpleFilter { Path = "CustomerFullName", Value = "Da", Type = FilterType.Contains }
                    }
                };

                var query = new QueryHandler();
                query.AddInterceptor(new MockFilterInterceptorAWithExtension());
                var result = query.Execute(queryable, criteria);

                var actual = result.Data;
                var expected = queryable.Where(t => t.Customer.FirstName.Contains("Da") || t.Customer.LastName.Contains("Da")).ToList();
                Assert.Equal(expected, actual);
            });
        }

        [Fact]
        public void Multi()
        {
            MockContextFactory.SeedAndTestContextFor("FilterInterceptorTests_Multi", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var queryable = ctx.Orders.AsQueryable();

                var criteria = new QueryCriteria()
                {
                    Filters = new List<IFilter>
                    {
                        new SimpleFilter { Path = "CustomerFirstName", Value = "David", Type = FilterType.Contains }
                    }
                };

                var query = new QueryHandler();
                query.AddInterceptor(new MockFilterInterceptorA());
                query.AddInterceptor(new MockFilterInterceptorB());
                var result = query.Execute(queryable, criteria);

                var actual = result.Data;
                var expected = queryable.Where(t => t.Customer.LastName == "Norris").ToList();
                Assert.Equal(expected, actual);
            });
        }
    }
}
