using System;
using System.Collections.Generic;
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
            MockContextFactory.SeedAndTestContextFor("CriteriaTests_TestEmptyCriteria", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var resultShouldMatch = ctx.Items.ToList();
                var queryable = ctx.Items.AsQueryable();

                // query handler that is empty should be the same as running to list.
                var criteria = new QueryCriteria();
                var queryHandler = new QueryHandler();
                var result = queryHandler.Execute(queryable, criteria);
                Assert.Equal(resultShouldMatch, result.Data);
            });
        }
    }
}
