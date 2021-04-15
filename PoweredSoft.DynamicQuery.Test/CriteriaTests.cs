using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class CriteriaTests
    {
        [Fact]
        public void TestEmptyCriteria()
        {
            MockContextFactory.SeedAndTestContextFor("CriteriaTests_TestEmptyCriteria", TestSeeders.SimpleSeedScenario,
                ctx =>
                {
                    var resultShouldMatch = ctx.Items.ToList();
                    var queryable = ctx.Items.AsQueryable();

                    // query handler that is empty should be the same as running to list.
                    var criteria = new QueryCriteria();
                    var queryHandler = new QueryHandler(Enumerable.Empty<IQueryInterceptorProvider>());
                    var result = queryHandler.Execute(queryable, criteria);
                    Assert.Equal(resultShouldMatch, result.Data.Cast<Item>().ToList());
                });
        }

        [Fact]
        public void TestPaging()
        {
            MockContextFactory.SeedAndTestContextFor("CriteriaTests_TestPagging", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var resultShouldMatch = ctx.OrderItems.OrderBy(t => t.Id).Skip(5).Take(5).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria();
                criteria.Sorts.Add(new Sort("Id"));
                criteria.Page = 2;
                criteria.PageSize = 5;

                var queryHandler = new QueryHandler(Enumerable.Empty<IQueryInterceptorProvider>());
                var result = queryHandler.Execute(ctx.OrderItems, criteria);
                var data = result.Data.Cast<OrderItem>().ToList();
                Assert.Equal(resultShouldMatch, data);
            });
        }

        [Fact]
        public void TestDI()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddPoweredSoftDynamicQuery();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            MockContextFactory.SeedAndTestContextFor("CriteriaTests_TestPagging", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var resultShouldMatch = ctx.OrderItems.OrderBy(t => t.Id).Skip(5).Take(5).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria();
                criteria.Sorts.Add(new Sort("Id"));
                criteria.Page = 2;
                criteria.PageSize = 5;
                var queryHandler = serviceProvider.GetService(typeof(IQueryHandler)) as IQueryHandler;
                var result = queryHandler.Execute(ctx.OrderItems, criteria);
                var data = result.Data.Cast<OrderItem>().ToList();
                Assert.Equal(resultShouldMatch, data);
            });
        }

        [Fact]
        public void TestCollection()
        {
            var serviceProvider = DIService();

            MockContextFactory.SeedAndTestContextFor("CriteriaTests_TestPagging", TestSeeders.SimpleSeedScenario, ctx =>
            {
                // var resultShouldMatch =
                //     ctx.Customers.Include(c => c.Orders); //OrderBy(t => t.Id).Skip(5).Take(5).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria();
                criteria.Filters.AddRange(new List<IFilter>
                {
                    // new SimpleFilter
                    // {
                    //     Path = "RegisterTime",
                    //     Value = "2018-10-1",
                    //     Type = FilterType.GreaterThanOrEqual,
                    //     And = true
                    // },
                    // new SimpleFilter
                    // {
                    //     Path = "RegisterTime",
                    //     Value = "2018-12-1",
                    //     Type = FilterType.LessThanOrEqual,
                    //     And = true
                    // },

                    new CompositeFilter()
                    {
                        And = true,
                        Type = FilterType.Composite,
                        Filters = new List<IFilter>
                        {
                            new SimpleFilter
                            {
                                And = true,
                                Path = "Orders.Date",
                                Value = "2018-01-03",
                                Type = FilterType.GreaterThanOrEqual
                            },
                            new SimpleFilter
                            {
                                And = true,
                                Path = "Orders.Date",
                                Value = "2018-01-04",
                                Type = FilterType.LessThanOrEqual
                            }
                        }
                    }
                });
                criteria.Sorts.Add(new Sort("Id"));
                criteria.Page = 1;
                criteria.PageSize = 15;

                var queryHandler = serviceProvider.GetService<IQueryHandler>();
                var result = queryHandler.Execute(ctx.Customers.Include(c => c.Orders), criteria);
                var data = result.Data.Cast<Customer>().ToList();
            });
        }

        [Fact]
        public void TestTimeSpanFilter()
        {
            var serviceProvider = DIService();

            MockContextFactory.SeedAndTestContextFor("CriteriaTests_TestPagging", TestSeeders.SimpleSeedScenario, ctx =>
            {
                // var resultShouldMatch =
                //     ctx.Customers.Include(c => c.Orders); //OrderBy(t => t.Id).Skip(5).Take(5).ToList();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria();
                criteria.Filters.AddRange(new List<IFilter>
                {
                    new CompositeFilter()
                    {
                        And = true,
                        Type = FilterType.Composite,
                        Filters = new List<IFilter>
                        {
                            new SimpleFilter
                            {
                                And = true,
                                Path = "birthTime",
                                Value = "12:00",
                                Type = FilterType.GreaterThanOrEqual
                            },
                            new SimpleFilter
                            {
                                And = true,
                                Path = "birthTime",
                                Value = "13:00",
                                Type = FilterType.LessThanOrEqual
                            }
                        }
                    }
                });
                criteria.Sorts.Add(new Sort("Id"));
                criteria.Page = 1;
                criteria.PageSize = 15;

                var queryHandler = serviceProvider.GetService<IQueryHandler>();
                var result = queryHandler.Execute(ctx.Customers.Include(c => c.Orders), criteria);
                var data = result.Data.Cast<Customer>().ToList();
                Assert.DoesNotContain(data,
                    c => c.BirthTime < TimeSpan.Parse("12:00") || c.BirthTime > TimeSpan.Parse("13:00"));
            });
        }

        private ServiceProvider DIService()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddPoweredSoftDynamicQuery();
            return serviceCollection.BuildServiceProvider();
        }
    }
}