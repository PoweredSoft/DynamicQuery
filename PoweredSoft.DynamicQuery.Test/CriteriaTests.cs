using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PoweredSoft.DynamicQuery.Test.Mock;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class CriteriaTests
    {
        [Fact]
        public void TestEmptyCriteria()
        {
            var options = new DbContextOptionsBuilder<MockContext>().UseInMemoryDatabase(databaseName: "TestEmptyCriteria").Options;
            using (var ctx = new MockContext(options))
            {
                ctx.AddRange(new Item[]
                {
                    new Item { Id = 1, Name = "Computer", Price = 1000M },
                    new Item { Id = 2, Name = "Mice", Price = 25.99M },
                    new Item { Id = 3, Name = "Keyboard", Price = 100M },
                    new Item { Id = 4, Name = "Screen", Price = 499.98M },
                    new Item { Id = 5, Name = "Printer", Price = 230.95M },
                });

                ctx.SaveChanges();
            }

            using (var ctx = new MockContext(options))
            {
                var resultShouldMatch = ctx.Items.ToList();
                var queryable = ctx.Items.AsQueryable();
                
                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria();
                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(queryable, criteria);
                Assert.All(resultShouldMatch, t => result.Data.Contains(t));
            }
        }
    }
}
