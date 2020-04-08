using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;
using Xunit.Sdk;

namespace PoweredSoft.DynamicQuery.Test.Mock
{
    public static class MockContextFactory
    {
        public static void TestContextFor(string testName, Action<MockContext> action)
        {
            var options = new DbContextOptionsBuilder<MockContext>()
                .UseInMemoryDatabase(databaseName: testName).Options;

            using var ctx = new MockContext(options);
                action(ctx);
        }

        public static void SeedAndTestContextFor(string testName, Action<string> seedAction, Action<MockContext> action)
        {
            seedAction(testName);
            TestContextFor(testName, action);
        }
        
    }
}
