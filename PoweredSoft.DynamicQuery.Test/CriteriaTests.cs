using System.Linq;
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
    }
}